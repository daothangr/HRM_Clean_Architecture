using HRM.Application.Overtime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OvertimeController : ControllerBase
{
    private readonly ISender _sender;

    public OvertimeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<OvertimeRequestDto>>> Get(CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetOvertimeRequestsQuery(), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateOvertimeRequestCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPost("{id:int}/process")]
    public async Task<IActionResult> Process(int id, [FromBody] ProcessBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new ProcessOvertimeRequestCommand(id, body.Approve, body.Comment), cancellationToken);
        return NoContent();
    }

    public record ProcessBody(bool Approve, string? Comment);
}
