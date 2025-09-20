using Application.DTOs;

namespace Application.Services
{
    public interface IAuthService
    {
        Task<TokenDto> RegisterAsync(UserRegisterDto dto, CancellationToken token);
        Task<TokenDto> LoginAsync(UserLoginDto dto, CancellationToken token);
        Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken token);
    }
}
