namespace InvoiceFlow.Application.DTOs;

public sealed record ApproveInvoiceRequest(
    string? Vendor,
    string? InvoiceNumber,
    DateOnly? InvoiceDate,
    decimal? Total,
    string? Currency
);