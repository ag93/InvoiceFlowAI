using InvoiceFlow.Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InvoicesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<InvoiceResponse>> GetInvoices()
    {
        var invoices = new List<InvoiceResponse>
        {
            new(
                Guid.Parse("c85b03cc-3223-43f8-8588-7635a7f3a768"),
                "Contoso",
                "INV-12345",
                new DateOnly(2026, 8, 12),
                1842.50m,
                "USD"
            ),
            new(
                Guid.Parse("e14ff654-97ec-40fd-a358-58da8f6be68d"),
                "Northwind Traders",
                "INV-67890",
                new DateOnly(2026, 9, 5),
                725.00m,
                "CAD"
            )
        };

        return Ok(invoices);
    }
}