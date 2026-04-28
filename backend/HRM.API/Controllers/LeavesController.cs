using HRM.Application.Common;
using HRM.Application.Leaves;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeavesController : ControllerBase
{
    private readonly ISender _sender;

    public LeavesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LeaveRequestDto>>> Get([FromQuery] GetLeaveRequestsQuery query, CancellationToken cancellationToken)
    {
        return await _sender.Send(query, cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPost("{id:int}/process")]
    public async Task<IActionResult> Process(int id, [FromBody] ProcessBody body, CancellationToken cancellationToken)
    {
        await _sender.Send(new ProcessLeaveRequestCommand(id, body.Approve, body.Comment), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CancelLeaveRequestCommand(id), cancellationToken);
        return NoContent();
    }

    public record ProcessBody(bool Approve, string? Comment);
}
