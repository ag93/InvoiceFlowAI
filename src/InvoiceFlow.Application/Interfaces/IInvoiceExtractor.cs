using InvoiceFlow.Application.DTOs;

namespace InvoiceFlow.Application.Interfaces;

public interface IInvoiceExtractor
{
    Task<InvoiceExtractionResult> ExtractAsync(Stream document, CancellationToken cancellationToken);
}