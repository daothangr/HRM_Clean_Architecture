using HRM.Application.Common.Constants;
using HRM.Application.Common;
using HRM.Application.WorkAttendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AttendanceRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceRecordsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AttendanceRecordDto>>> Get(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? employeeCode,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return await _sender.Send(new GetAttendanceRecordsQuery(from, to, employeeCode, pageNumber, pageSize), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Upsert([FromBody] UpsertAttendanceCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAttendanceCommand(id), cancellationToken);
        return NoContent();
    }
}
