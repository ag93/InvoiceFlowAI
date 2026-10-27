using InvoiceFlow.Api.Contracts.Responses;
using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using InvoiceFlow.Application.Services;
using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentsController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly IDocumentStorage _documentStorage;
    private readonly IDocumentRepository _documentRepository;
    private readonly InvoiceProcessingService _processingService;
    private readonly InvoiceApprovalService _approvalService;
    public DocumentsController(IDocumentStorage documentStorage,
                               InvoiceProcessingService processingService,
                               IDocumentRepository documentRepository,
                               InvoiceApprovalService approvalService)
    {
        _documentStorage = documentStorage;
        _processingService = processingService;
        _documentRepository = documentRepository;
        _approvalService = approvalService;
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

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = Path.GetFileName(file.FileName),
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _documentRepository.CreateAsync(document, cancellationToken);
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

    [HttpPost("{id:guid}/analyze")]
    [ProducesResponseType(typeof(InvoiceProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var result = await _processingService.ProcessAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                Message = "Document not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    //[ProducesResponseType(typeof(Application.DTOs.InvoiceResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _approvalService.ApproveAsync(
            id,
            request,
            cancellationToken);

        return result.Status switch
        {
            InvoiceApprovalStatus.Approved =>
                Ok(result.Invoice),

            InvoiceApprovalStatus.Invalid =>
                BadRequest(result.Validation),

            InvoiceApprovalStatus.NotFound =>
                NotFound(new
                {
                    Message = "Document not found."
                }),

            InvoiceApprovalStatus.Conflict =>
                Conflict(new
                {
                    Message = "Document is not eligible for approval or is already approved."
                }),

            _ => StatusCode(500)
        };
    }
}

//https://localhost:44360/api/Documents/140884f4-68a9-4122-b10a-7896042fe2ad/file