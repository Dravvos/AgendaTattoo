namespace AgendaTattoo.BLL.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Chave simétrica (base64) usada para assinar/validar os tokens.
    /// NUNCA deve vir do appsettings.json versionado — sempre de user-secrets (dev)
    /// ou variável de ambiente / cofre de segredos (produção).
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
