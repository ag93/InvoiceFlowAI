using InvoiceFlow.Application.DTOs;

namespace InvoiceFlow.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<InvoiceResponse?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken);
}