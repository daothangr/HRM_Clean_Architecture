using HRM.Application.Common.Constants;
using HRM.Application.Departments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public DepartmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> Get(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        return await _sender.Send(new GetDepartmentsQuery(includeInactive), cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        return await _sender.Send(new GetDepartmentByIdQuery(id), cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<ActionResult<int>> Create([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id and body id must match.");
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteDepartmentCommand(id), cancellationToken);
        return NoContent();
    }
}
