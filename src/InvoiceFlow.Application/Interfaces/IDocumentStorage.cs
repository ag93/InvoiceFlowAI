namespace InvoiceFlow.Application.Interfaces;

public interface IDocumentStorage
{
    Task<Guid> SaveAsync(Stream content, CancellationToken cancellationToken);
    Task<Stream?> OpenReadAsync(Guid documentId, CancellationToken cancellationToken);
}