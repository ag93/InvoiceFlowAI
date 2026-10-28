using InvoiceFlow.Application.DTOs;
using InvoiceFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace InvoiceFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiredScope("Invoices.Access")]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoicesController(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);

        if (invoice is null)
        {
            return NotFound(new
            {
                Message = "Invoice not found."
            });
        }

        return Ok(invoice);
    }
}