namespace InvoiceFlow.Application.Interfaces;

public interface IDocumentStorage
{
    Task<Guid> SaveAsync(Stream content, CancellationToken cancellationToken);
}