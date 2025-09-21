using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "username": "testuser",
    ///   "email": "test@example.com",
    ///   "password": "Password123!"
    /// }
    /// </remarks>
    /// <param name="dto">User registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns JWT access and refresh tokens</response>
    /// <response code="400">User already exists or invalid input</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.RegisterAsync(dto, cancellationToken);
            return Ok(token);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("User exists"))
            {
                return BadRequest(new { error = ex.Message });
            }
            throw;
        }
    }

    /// <summary>
    /// Login user and return JWT tokens
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "email": "test@example.com",
    ///   "password": "Password123!"
    /// }
    /// </remarks>
    /// <param name="dto">User login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns JWT access and refresh tokens</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.LoginAsync(dto, cancellationToken);
            return Ok(token);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Invalid credentials"))
            {
                return Unauthorized(new { error = ex.Message });
            }
            throw;
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "refreshToken": "your-refresh-token"
    /// }
    /// </remarks>
    /// <param name="refreshToken">Refresh token details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns new JWT access and refresh tokens</response>
    /// <response code="401">Invalid refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
            return Ok(token);
        }
        catch (Exception)
        {
            throw;
        }

    }
}