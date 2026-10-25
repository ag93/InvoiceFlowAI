using InvoiceFlow.Application.DTOs;

namespace InvoiceFlow.Application.Validation;

public sealed class InvoiceValidationService
{
    public InvoiceValidationResult Validate(InvoiceExtractionResult invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var issues = new List<ValidationIssue>();

        // 1. Vendor
        if (string.IsNullOrWhiteSpace(invoice.Vendor))
        {
            issues.Add(new ValidationIssue("Vendor", "Vendor is required."));
        }

        // 2. Invoice number
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            issues.Add(new ValidationIssue("InvoiceNumber", "Invoice number is required."));
        }

        // 3. Invoice date
        if (invoice.Date is null)
        {
            issues.Add(new ValidationIssue("Date", "Invoice date is required."));
        }

        // 4. Total amount
        if (invoice.Total is null || invoice.Total <= 0)
        {
            issues.Add(new ValidationIssue("Total", "Total must be greater than zero."));
        }

        // 5. Currency format
        if (invoice.Currency is null ||
            invoice.Currency.Length != 3 ||
            !invoice.Currency.All(c => c is >= 'A' and <= 'Z'))
        {
            issues.Add(new ValidationIssue("Currency", "Currency must be a three-letter uppercase code."));
        }

        return new InvoiceValidationResult(issues);
    }
}