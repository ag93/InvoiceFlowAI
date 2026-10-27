using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> TryStartProcessingAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents.FindAsync(new object[] { documentId }, cancellationToken);

        if (document is null)
        {
            return false;
        }

        document.Status = DocumentStatus.Processing;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveExtractionAsync(Guid documentId, InvoiceExtractionResult extraction, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents.FindAsync(new object[] { documentId }, cancellationToken);

        if (document is null)
        {
            throw new InvalidOperationException("Document record no longer exists.");
        }

        var extractionEntity = new ExtractionResult
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Vendor = extraction.Vendor,
            InvoiceNumber = extraction.InvoiceNumber,
            InvoiceDate = extraction.Date,
            Total = extraction.Total,
            Currency = extraction.Currency,
            ExtractedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ExtractionResults.Add(extractionEntity);
        document.Status = DocumentStatus.NeedsReview;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents.FindAsync(new object[] { documentId }, cancellationToken);

        if (document is null)
        {
            return;
        }

        document.Status = DocumentStatus.Failed;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<InvoiceApprovalResult> ApproveAsync(
    Guid documentId,
    InvoiceExtractionResult correctedValues,
    CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents.FindAsync(new object[] { documentId }, cancellationToken);

        if (document is null)
        {
            return new InvoiceApprovalResult(InvoiceApprovalStatus.NotFound);
        }

        if (document.Status != DocumentStatus.NeedsReview)
        {
            return new InvoiceApprovalResult(InvoiceApprovalStatus.Conflict);
        }

        // Defense against approving a document twice.
        var alreadyApproved = await _dbContext.Invoices.AnyAsync(x => x.DocumentId == documentId, cancellationToken);

        if (alreadyApproved)
        {
            return new InvoiceApprovalResult(InvoiceApprovalStatus.Conflict);
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Vendor = correctedValues.Vendor!,
            InvoiceNumber = correctedValues.InvoiceNumber!,
            InvoiceDate = correctedValues.Date!.Value,
            Total = correctedValues.Total!.Value,
            Currency = correctedValues.Currency!,
            ApprovedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Invoices.Add(invoice);

        document.Status = DocumentStatus.Approved;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new InvoiceResponse(
            invoice.Id,
            invoice.DocumentId,
            invoice.Vendor,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.Total,
            invoice.Currency,
            invoice.ApprovedAt);

        return new InvoiceApprovalResult(InvoiceApprovalStatus.Approved, Invoice: response);
    }
}