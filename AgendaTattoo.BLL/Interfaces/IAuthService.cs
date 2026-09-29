using AgendaTattoo.BLL.Models;
using AgendaTattoo.DTO.Auth;

namespace AgendaTattoo.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterStudioAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Gera o token de redefinição e "envia" o e-mail (via IEmailSender). Nunca lança erro nem
    /// indica se o e-mail existe ou não na base — evita enumeração de contas, mesma lógica já
    /// usada em LoginAsync.
    /// </summary>
    Task RequestPasswordResetAsync(string email, CancellationToken ct = default);

    /// <summary>Efetiva a redefinição com o token recebido por e-mail. Revoga sessões ativas ao suceder.</summary>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);

}
