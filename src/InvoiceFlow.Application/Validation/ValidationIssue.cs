namespace InvoiceFlow.Application.Validation;

public sealed record ValidationIssue(string Field, string Message
);