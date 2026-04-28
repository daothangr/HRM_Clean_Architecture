using HRM.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest body, CancellationToken cancellationToken)
    {
        return await _sender.Send(new LoginCommand(body.Email, body.Password), cancellationToken);
    }

    public record LoginRequest(string Email, string Password);
}
