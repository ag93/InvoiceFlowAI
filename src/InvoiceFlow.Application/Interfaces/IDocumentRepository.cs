using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Domain.Entities;

namespace InvoiceFlow.Application.Interfaces;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken cancellationToken);
    Task<StartProcessingResult> TryStartProcessingAsync(Guid documentId, CancellationToken cancellationToken);
    Task SaveExtractionAsync(Guid documentId, InvoiceExtractionResult extraction, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid documentId, CancellationToken cancellationToken);
    Task<InvoiceApprovalResult> ApproveAsync(Guid documentId, InvoiceExtractionResult correctedValues, CancellationToken cancellationToken);
}