using InvoiceFlow.Domain.Entities;

namespace InvoiceFlow.Application.Interfaces;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken cancellationToken);
}