using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EvoConnect.Server.Repository.Interfaces;
using System.Security;
namespace EvoConnect.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authRepository.AuthenticateAsync(request.Username, request.Password);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }
        var gestion =result.User!.Gestion?.Trim() ;
        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User?.IdUtil,
                username = result.User?.Identifiant,
                personneId = result.User?.IdPersonne,
                gestion = string.IsNullOrWhiteSpace(gestion) ? "ASSIS" : gestion,
            }
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _authRepository.GetAuthenticatedUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _authRepository.GetUserByIdAsync(userId.Value);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}