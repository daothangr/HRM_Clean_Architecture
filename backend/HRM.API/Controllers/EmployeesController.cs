using HRM.Application.Common.Constants;
using HRM.Application.Common;
using HRM.Application.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await _sender.Send(new GetEmployeesQuery(pageNumber, pageSize), cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);
    }

    [HttpGet("managers")]
    public async Task<ActionResult<List<EmployeeListDto>>> GetManagers(CancellationToken cancellationToken)
    {
        return await _sender.Send(new GetManagersQuery(), cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<ActionResult<int>> Create([FromBody] CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeCommand command, CancellationToken cancellationToken)
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
        await _sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }
}
