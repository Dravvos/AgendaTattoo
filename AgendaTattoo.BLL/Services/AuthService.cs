using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.BLL.Models;
using AgendaTattoo.BLL.Security;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgendaTattoo.BLL.Services;

public class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "E-mail ou senha inválidos.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AgendaTattooDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        AgendaTattooDbContext db,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterStudioAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var slug = request.StudioSlug.Trim().ToLowerInvariant();

        var slugTaken = await _db.Studios.AnyAsync(s => s.Slug == slug, ct);
        if (slugTaken)
            return AuthResult.Fail("Esse identificador de estúdio já está em uso.");

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var studio = new Studio
        {
            Id = Guid.NewGuid(),
            Name = request.StudioName.Trim(),
            Slug = slug,
        };
        _db.Studios.Add(studio);
        await _db.SaveChangesAsync(ct);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.OwnerFullName.Trim(),
            StudioId = studio.Id,
            // TODO: quando houver envio de e-mail, trocar para false + fluxo de confirmação.
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(ct);
            return AuthResult.Fail(createResult.Errors.Select(e => e.Description).ToArray());
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Owner))
            await _roleManager.CreateAsync(new ApplicationRole(Roles.Owner));

        await _userManager.AddToRoleAsync(user, Roles.Owner);

        await transaction.CommitAsync(ct);

        return await IssueTokensAsync(user, "registration", ct);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        // Mesma mensagem genérica tanto para "usuário não existe" quanto para "senha errada":
        // evita que um atacante descubra quais e-mails estão cadastrados (user enumeration).
        if (user is null || !user.IsActive)
            return AuthResult.Fail(InvalidCredentialsMessage);

        var checkResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (checkResult.IsLockedOut)
            return AuthResult.Fail("Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.");

        if (!checkResult.Succeeded)
            return AuthResult.Fail(InvalidCredentialsMessage);

        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored is null)
            return AuthResult.Fail("Refresh token inválido.");

        if (stored.RevokedAt is not null)
        {
            // Um refresh token já usado/revogado sendo reapresentado é um forte indício de roubo de token.
            // Por segurança, revoga TODAS as sessões ativas do usuário, não só esta.
            await RevokeAllActiveTokensAsync(stored.UserId, ct);
            return AuthResult.Fail("Refresh token inválido.");
        }

        if (!stored.IsActive)
            return AuthResult.Fail("Refresh token expirado.");

        stored.RevokedAt = DateTimeOffset.UtcNow;

        var result = await IssueTokensAsync(stored.User, ipAddress, ct);

        stored.ReplacedByTokenHash = _tokenService.HashToken(result.RefreshToken!);
        await _db.SaveChangesAsync(ct);

        return result;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task RevokeAllActiveTokensAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    private async Task<AuthResult> IssueTokensAsync(ApplicationUser user, string ipAddress, CancellationToken ct)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = ipAddress,
        });
        await _db.SaveChangesAsync(ct);

        return AuthResult.Success(accessToken, expiresAt, refreshToken);
    }

}
