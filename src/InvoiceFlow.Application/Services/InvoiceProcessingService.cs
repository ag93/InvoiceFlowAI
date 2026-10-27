using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Validation;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.Application.Services;

public sealed class InvoiceProcessingService
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IInvoiceExtractor _invoiceExtractor;
    private readonly InvoiceValidationService _validator;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<InvoiceProcessingService> _logger;

    public InvoiceProcessingService(
        IDocumentStorage documentStorage,
        IInvoiceExtractor invoiceExtractor,
        InvoiceValidationService validator,
        IDocumentRepository documentRepository,
        ILogger<InvoiceProcessingService> logger)
    {
        _documentStorage = documentStorage;
        _invoiceExtractor = invoiceExtractor;
        _validator = validator;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<InvoiceProcessingResult?> ProcessAsync(Guid documentId, CancellationToken cancellationToken)
    {
        // 1. Retrieve the original PDF.
        await using var stream = await _documentStorage.OpenReadAsync(documentId, cancellationToken);

        if (stream is null)
        {
            return null;
        }

        // 2. Confirm the document exists in SQLite.
        var started = await _documentRepository.TryStartProcessingAsync(documentId, cancellationToken);

        if (!started)
        {
            return null;
        }

        try
        {
            // 3. Extract candidate invoice values.
            var extraction = await _invoiceExtractor.ExtractAsync(stream, cancellationToken);

            // 4. Apply business validation rules.
            var validation = _validator.Validate(extraction);

            // 5. Persist the extraction and review status.
            await _documentRepository.SaveExtractionAsync(documentId, extraction, cancellationToken);

            return new InvoiceProcessingResult(documentId, extraction, validation);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Invoice processing failed for document {DocumentId}",
                documentId);

            // Preserve the original failure if updating
            // the database status also fails.
            try
            {
                await _documentRepository.MarkFailedAsync(documentId, CancellationToken.None);
            }
            catch (Exception statusException)
            {
                _logger.LogError(statusException, "Could not mark document {DocumentId} as failed", documentId);
            }

            throw;
        }
    }
}