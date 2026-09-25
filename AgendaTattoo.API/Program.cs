using AgendaTattoo.API.Infrastructure;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.BLL.Security;
using AgendaTattoo.BLL.Services;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// Add services to the container.
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção 'Jwt' não configurada em appsettings/user-secrets.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey não configurada. Defina via 'dotnet user-secrets' (dev) ou variável de " +
        "ambiente (produção) — nunca em appsettings.json versionado. Veja SETUP-AUTH.md.");
}

// ---------- Banco de dados ----------

builder.Services.AddDbContext<AgendaTattooDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Identity ----------
// AddIdentityCore (em vez de AddIdentity) porque esta é uma API pura: não usamos
// autenticação por cookie, só emitimos/validamos JWTs.

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Política de senha.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Bloqueio de conta após tentativas falhas — mitigação de força bruta.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;

        // TODO: habilitar quando houver envio de e-mail de confirmação.
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AgendaTattooDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ---------- Autenticação (JWT Bearer) ----------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false; // não guarda o token bruto no HttpContext além do necessário

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero, // sem tolerância extra de expiração
        };
    });

// ---------- Autorização ----------
// Política padrão: TODA a API exige usuário autenticado. Endpoints públicos
// (login, registro, e no futuro a agenda pública do estúdio) precisam de
// [AllowAnonymous] explícito — ou seja, aberto é a exceção, não a regra.

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("OwnerOnly", policy => policy.RequireRole(Roles.Owner))
    .AddPolicy("StudioMember", policy => policy.RequireRole(Roles.Owner, Roles.Artist));

// ---------- Rate limiting ----------
// Limita tentativas de login/refresh por janela de tempo — mitigação extra de
// força bruta e credential stuffing, complementar ao lockout do Identity.

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // Navegação na agenda pública (consultar estúdio, serviços, horários livres etc.).
    options.AddFixedWindowLimiter("public", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // Criar um agendamento é a ação pública mais sensível a abuso/spam — limite bem mais apertado.
    options.AddFixedWindowLimiter("public-booking", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

// ---------- CORS ----------
// O frontend está fora do escopo deste momento do projeto. Quando existir,
// configure as origens permitidas em Cors:AllowedOrigins (appsettings/env) —
// nunca com wildcard "*" quando AllowCredentials estiver ativo.

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            // Nenhuma origem configurada => nenhum navegador terá permissão de CORS.
            // (chamadas server-to-server continuam funcionando normalmente, CORS é uma
            // proteção aplicada pelo navegador do lado do cliente.)
            policy.DisallowCredentials();
        }
    });
});

// ---------- Serviços de aplicação ----------

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPublicBookingService, PublicBookingService>();

// Traduz exceções de domínio (NotFound/Conflict/Forbidden/ValidationApp) em respostas
// HTTP consistentes, sem precisar de try/catch repetido em cada controller.
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();

var app = builder.Build();

// Precisa vir antes de qualquer outro middleware que possa lançar exceção de domínio.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
    
}
else
{
    app.UseHsts();
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
