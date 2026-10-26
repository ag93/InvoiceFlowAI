using InvoiceFlow.Application.Validation;

namespace InvoiceFlow.Application.DTOs;

public sealed record InvoiceProcessingResult(
    Guid DocumentId,
    InvoiceExtractionResult Extraction,
    InvoiceValidationResult Validation
);