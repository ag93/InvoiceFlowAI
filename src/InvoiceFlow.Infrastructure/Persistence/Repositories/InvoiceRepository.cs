using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _dbContext;

    public InvoiceRepository(InvoiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var invoices = await _dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(x => x.InvoiceDate)
            .ThenBy(x => x.InvoiceNumber)
            .ToListAsync(cancellationToken);

        return invoices
            .Select(ToResponse)
            .ToList();
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == invoiceId,
                cancellationToken);

        return invoice is null
            ? null
            : ToResponse(invoice);
    }

    private static InvoiceResponse ToResponse(Invoice invoice)
    {
        return new InvoiceResponse(
            invoice.Id,
            invoice.DocumentId,
            invoice.Vendor,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.Total,
            invoice.Currency,
            invoice.ApprovedAt);
    }
}