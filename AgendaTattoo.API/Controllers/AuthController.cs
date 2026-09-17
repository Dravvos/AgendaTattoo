using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.BLL.Models;
using AgendaTattoo.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Cria um novo estúdio e o usuário dono (Owner) que o administra.</summary>
        [HttpPost("register-studio")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> RegisterStudio([FromBody] DTO.Auth.RegisterRequest request, CancellationToken ct)
        {
            var result = await _authService.RegisterStudioAsync(request, ct);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(ToResponse(result));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] DTO.Auth.LoginRequest request, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(request, ip, ct);

            if (!result.Succeeded)
                return Unauthorized(new { errors = result.Errors });

            return Ok(ToResponse(result));
        }

        /// <summary>Troca um refresh token válido por um novo par access/refresh token (com rotação).</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ip, ct);

            if (!result.Succeeded)
                return Unauthorized(new { errors = result.Errors });

            return Ok(ToResponse(result));
        }

        /// <summary>Revoga o refresh token informado (logout). Requer usuário autenticado.</summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken, ct);
            return NoContent();
        }

        private static AuthResponse ToResponse(AuthResult result) => new()
        {
            AccessToken = result.AccessToken!,
            AccessTokenExpiresAt = result.AccessTokenExpiresAt!.Value,
            RefreshToken = result.RefreshToken!,
        };
    }
}
