using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Domain.Entities;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly InvoiceDbContext _dbContext;

    public DocumentRepository(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(Document document, CancellationToken cancellationToken)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}