using LoginSRV1.DTOs;

namespace LoginSRV1.Services
{
    public interface IAuthService
    {
        Task<AuthOperationResult<LoginSuccessResponseDto>> LoginAsync(string? usuario, string? password, string? tipo);
        Task<AuthOperationResult<RefreshResponseDto>> RefreshTokenAsync(string? refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string? token);
    }
}
