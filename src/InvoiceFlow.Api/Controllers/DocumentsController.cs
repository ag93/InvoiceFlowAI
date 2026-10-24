using InvoiceFlow.Api.Contracts.Responses;
using InvoiceFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentsController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly IDocumentStorage _documentStorage;
    public DocumentsController(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentUploadResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Please upload a PDF file.");
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest("File size cannot exceed 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are supported.");
        }

        byte[] signature = new byte[5];

        await using var stream = file.OpenReadStream();
        int bytesRead = await stream.ReadAsync(signature.AsMemory(), cancellationToken);

        byte[] expectedSignature =
        [
            0x25, // %
            0x50, // P
            0x44, // D
            0x46, // F
            0x2D  // -
        ];

        if (bytesRead != expectedSignature.Length ||
            !signature.SequenceEqual(expectedSignature))
        {
            return BadRequest("The file does not have a valid PDF signature.");
        }

        await using var content = file.OpenReadStream();

        var documentId = await _documentStorage.SaveAsync(content, cancellationToken);

        var response = new DocumentUploadResponse(documentId, file.FileName, file.Length,
            "application/pdf", "PDF uploaded and stored successfully.");

        return Ok(response);
    }

    [HttpGet("{id:guid}/file")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var stream = await _documentStorage.OpenReadAsync(id, cancellationToken);

        if (stream is null)
        {
            return NotFound(new
            {
                Message = "Document not found.",
                DocumentId = id
            });
        }

        return File(stream, "application/pdf");
    }
}

//https://localhost:44360/api/Documents/22643e06-3ff0-4e92-8d4d-1764bd083d67/file