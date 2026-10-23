namespace InvoiceFlow.Api.Contracts.Responses;

public sealed record DocumentUploadResponse(Guid DocumentId, string FileName, long SizeInBytes, 
    string ContentType, string Message);