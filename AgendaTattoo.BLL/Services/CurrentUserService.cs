using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgendaTattoo.BLL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AgendaTattoo.BLL.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;

    public Guid? StudioId =>
        Guid.TryParse(User?.FindFirst("studio_id")?.Value, out var id) ? id : null;

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
}
