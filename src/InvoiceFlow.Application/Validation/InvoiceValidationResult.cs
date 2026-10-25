namespace InvoiceFlow.Application.Validation;

public sealed record InvoiceValidationResult(IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsValid => Issues.Count == 0;
}