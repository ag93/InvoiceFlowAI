using InvoiceFlow.Application.Validation;

namespace InvoiceFlow.Application.DTOs;

public enum InvoiceApprovalStatus
{
    Approved,
    Invalid,
    NotFound,
    Conflict
}

public sealed record InvoiceApprovalResult(InvoiceApprovalStatus Status,
                                           InvoiceResponse? Invoice = null,
                                           InvoiceValidationResult? Validation = null);