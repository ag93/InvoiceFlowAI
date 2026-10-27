using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Validation;

namespace InvoiceFlow.Application.Services;

public sealed class InvoiceApprovalService
{
    private readonly IDocumentRepository _repository;
    private readonly InvoiceValidationService _validator;

    public InvoiceApprovalService(IDocumentRepository repository, InvoiceValidationService validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<InvoiceApprovalResult> ApproveAsync(Guid documentId, ApproveInvoiceRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Reuse the existing validation contract.
        var correctedValues = new InvoiceExtractionResult(
            request.Vendor,
            request.InvoiceNumber,
            request.InvoiceDate,
            request.Total,
            request.Currency);

        var validation = _validator.Validate(correctedValues);

        if (request.Total is decimal total && decimal.Round(total, 2) != total)
        {
            validation = new InvoiceValidationResult(validation.Issues.Append(
                new ValidationIssue("Total", "Total cannot have more than two decimal places.")).ToList());
        }

        if (!validation.IsValid)
        {
            return new InvoiceApprovalResult(InvoiceApprovalStatus.Invalid, Validation: validation);
        }

        // The repository checks document eligibility
        // and performs the database write.
        return await _repository.ApproveAsync(documentId, correctedValues, cancellationToken);
    }
}