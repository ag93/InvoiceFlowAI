namespace InvoiceFlow.Application.DTOs;

public sealed record InvoiceResponse(
    Guid Id,
    Guid DocumentId,
    string Vendor,
    string InvoiceNumber,
    DateOnly InvoiceDate,
    decimal Total,
    string Currency,
    DateTimeOffset ApprovedAt
);