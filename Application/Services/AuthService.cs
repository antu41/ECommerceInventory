using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<TokenDto> RegisterAsync(UserRegisterDto dto, CancellationToken token)
        {
            try
            {
                var existing = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, token);
                if (existing != null) throw new Exception("User exists");

                var user = new User
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                await _unitOfWork.Users.AddAsync(user, token);
                await _unitOfWork.SaveChangesAsync(token);

                return GenerateTokens(user, token);

            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<TokenDto> LoginAsync(UserLoginDto dto, CancellationToken token)
        {
            try
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, token);
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) throw new Exception("Invalid credentials");

                return GenerateTokens(user, token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken token)
        {
            try
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow, token);
                if (user == null) throw new Exception("Invalid refresh token");

                return GenerateTokens(user, token);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private TokenDto GenerateTokens(User user, CancellationToken token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

                var accessToken = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!)),
                    signingCredentials: creds
                );

                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenExpiryDays"]!));
                _unitOfWork.Users.Update(user);
                _unitOfWork.SaveChangesAsync(token).Wait(); // Sync for simplicity

                return new TokenDto
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    RefreshToken = refreshToken
                };
            }
            catch (Exception)
            {
                throw;

            }
        }
    }
}
