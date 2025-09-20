using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _authService.RegisterAsync(dto, cancellationToken);
                return Ok(token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _authService.LoginAsync(dto, cancellationToken);
                return Ok(token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [HttpPost("refresh")]
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
}
