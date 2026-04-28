using HRM.Application.Common.Constants;
using HRM.Application.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<ActionResult<List<RoleDto>>> Get(CancellationToken cancellationToken = default)
    {
        return await _sender.Send(new GetRolesQuery(), cancellationToken);
    }
}
