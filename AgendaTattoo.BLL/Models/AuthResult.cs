namespace AgendaTattoo.BLL.Models;

public class AuthResult
{
    public bool Succeeded { get; init; }
    public string? AccessToken { get; init; }
    public DateTimeOffset? AccessTokenExpiresAt { get; init; }
    public string? RefreshToken { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static AuthResult Success(string accessToken, DateTimeOffset expiresAt, string refreshToken) => new()
    {
        Succeeded = true,
        AccessToken = accessToken,
        AccessTokenExpiresAt = expiresAt,
        RefreshToken = refreshToken,
    };

    public static AuthResult Fail(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors,
    };
}
