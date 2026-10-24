namespace InvoiceFlow.Application.DTOs;

public sealed record InvoiceExtractionResult(
    string? Vendor,
    string? InvoiceNumber,
    DateOnly? Date,
    decimal? Total,
    string? Currency
);