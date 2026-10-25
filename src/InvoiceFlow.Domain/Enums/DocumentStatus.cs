namespace InvoiceFlow.Domain.Enums;

public enum DocumentStatus
{
    Uploaded = 0,
    Processing = 1,
    NeedsReview = 2,
    Approved = 3,
    Failed = 4
}