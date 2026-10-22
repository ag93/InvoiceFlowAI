namespace InvoiceFlow.Api.Contracts.Responses;

public sealed record InvoiceResponse(Guid Id, string Vendor, string InvoiceNumber, DateOnly Date, decimal Total, string Currency);