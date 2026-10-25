namespace InvoiceFlow.Domain.Entities;

public sealed class ExtractionResult
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public decimal? Total { get; set; }
    public string? Currency { get; set; }
    public DateTimeOffset ExtractedAt { get; set; } = DateTimeOffset.UtcNow;
}