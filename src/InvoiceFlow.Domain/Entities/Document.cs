using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<ExtractionResult> Extractions { get; set; } = new List<ExtractionResult>();
    public Invoice? Invoice { get; set; }
}