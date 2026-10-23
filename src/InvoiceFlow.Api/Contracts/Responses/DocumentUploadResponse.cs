namespace InvoiceFlow.Api.Contracts.Responses;

public sealed record DocumentUploadResponse(string FileName, long SizeInBytes, string ContentType, string Message);