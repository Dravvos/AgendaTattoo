using AgendaTattoo.Data.Entities;

namespace AgendaTattoo.BLL.Interfaces;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(ApplicationUser user, IList<string> roles);

    /// <summary>Gera um refresh token opaco (não é um JWT) — valor aleatório de alta entropia.</summary>
    string GenerateRefreshToken();

    /// <summary>Hash de um token (SHA-256) para armazenamento seguro no banco.</summary>
    string HashToken(string token);
}
