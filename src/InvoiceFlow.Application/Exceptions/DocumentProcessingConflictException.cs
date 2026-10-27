namespace InvoiceFlow.Application.Exceptions;

public sealed class DocumentProcessingConflictException : Exception
{
    public DocumentProcessingConflictException()
        : base("Document is already processing or has been approved.")
    {
    }
}