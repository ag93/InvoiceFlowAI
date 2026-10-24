using Azure;
using Azure.AI.ContentUnderstanding;
using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;

namespace InvoiceFlow.Infrastructure.AI;

public sealed class AzureInvoiceExtractor : IInvoiceExtractor
{
    private readonly ContentUnderstandingClient _client;

    public AzureInvoiceExtractor(ContentUnderstandingClient client)
    {
        _client = client;
    }

    public async Task<InvoiceExtractionResult> ExtractAsync(Stream document, CancellationToken cancellationToken)
    {
        // 1. Convert the document stream.
        var binaryData = await BinaryData.FromStreamAsync(document, cancellationToken);

        // 2. Submit it to Azure AI.
        var operation = await _client.AnalyzeBinaryAsync(
            WaitUntil.Completed,
            "prebuilt-invoice",
            binaryData,
            contentType: "application/pdf",
            cancellationToken: cancellationToken);

        // 3. Obtain the document result.
        var result = operation.Value;

        var content = result.Contents.OfType<DocumentContent>().FirstOrDefault();

        if (content is null)
        {
            throw new InvalidOperationException("Azure returned no document content.");
        }

        // 4. Map the fields to application types.
        var fields = content.Fields;

        var vendor = fields.GetFieldOrDefault("VendorName")?.Value as string;

        var invoiceNumber = fields.GetFieldOrDefault("InvoiceId")?.Value as string;

        var dateField = fields.GetFieldOrDefault("InvoiceDate") as ContentDateTimeOffsetField;

        DateOnly? invoiceDate = dateField?.Value is DateTimeOffset date ? DateOnly.FromDateTime(date.Date) : null;

        var totalField = fields.GetFieldOrDefault("TotalAmount") as ContentObjectField;

        var amountField = totalField?.Value.GetFieldOrDefault("Amount") as ContentNumberField;

        var currencyField = totalField?.Value.GetFieldOrDefault("CurrencyCode") as ContentStringField;

        decimal? total = amountField?.Value is double amount ? Convert.ToDecimal(amount) : null;

        var currency = currencyField?.Value;

        // 5. Return an Azure-independent result.
        return new InvoiceExtractionResult(vendor, invoiceNumber, invoiceDate, total, currency);
    }
}