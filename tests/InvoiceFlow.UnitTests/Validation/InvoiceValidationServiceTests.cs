using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Validation;

namespace InvoiceFlow.UnitTests.Validation;

public class InvoiceValidationServiceTests
{
    private readonly InvoiceValidationService _validator = new();

    private static InvoiceExtractionResult ValidInvoice() => new(
        "Fictional Gulf Foods LLC",
        "INV-2026-001",
        new DateOnly(2026, 8, 1),
        63m,
        "AED");

    [Fact]
    public void Validate_ValidInvoice_ReturnsNoIssues()
    {
        // Arrange
        var invoice = ValidInvoice();

        // Act
        var result = _validator.Validate(invoice);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_MissingVendor_ReturnsVendorIssue()
    {
        var invoice = ValidInvoice() with { Vendor = null };

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Field == "Vendor");
    }

    [Fact]
    public void Validate_MissingInvoiceNumber_ReturnsIssue()
    {
        var invoice = ValidInvoice() with { InvoiceNumber = "" };

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Field == "InvoiceNumber");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_NonPositiveTotal_ReturnsTotalIssue(
        int total)
    {
        var invoice = ValidInvoice() with { Total = total };

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Field == "Total");
    }

    [Fact]
    public void Validate_MultipleInvalidFields_ReturnsAllIssues()
    {
        var invoice = new InvoiceExtractionResult(null, "", null, -10m, "US");

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Equal(5, result.Issues.Count);
    }
}