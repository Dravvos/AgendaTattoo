using AgendaTattoo.BLL.Models;
using AgendaTattoo.DTO.Auth;

namespace AgendaTattoo.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterStudioAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
