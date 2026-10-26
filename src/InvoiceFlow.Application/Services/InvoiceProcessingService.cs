using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Validation;

namespace InvoiceFlow.Application.Services;

public sealed class InvoiceProcessingService
{
    private readonly IDocumentStorage _documentStorage;
    private readonly IInvoiceExtractor _invoiceExtractor;
    private readonly InvoiceValidationService _validator;

    public InvoiceProcessingService(
        IDocumentStorage documentStorage,
        IInvoiceExtractor invoiceExtractor,
        InvoiceValidationService validator)
    {
        _documentStorage = documentStorage;
        _invoiceExtractor = invoiceExtractor;
        _validator = validator;
    }

    public async Task<InvoiceProcessingResult?> ProcessAsync(Guid documentId, CancellationToken cancellationToken)
    {
        // 1. Retrieve the stored PDF.
        await using var documentStream = await _documentStorage.OpenReadAsync(documentId, cancellationToken);

        if (documentStream is null)
        {
            return null;
        }

        // 2. Extract invoice information using Azure AI.
        var extraction = await _invoiceExtractor.ExtractAsync(documentStream, cancellationToken);

        // 3. Validate the extracted invoice fields.
        var validation = _validator.Validate(extraction);

        // 4. Return the combined result.
        return new InvoiceProcessingResult(documentId, extraction, validation);
    }
}