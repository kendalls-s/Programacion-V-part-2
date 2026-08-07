using LoginSRV1.Config;
using LoginSRV1.DTOs;
using LoginSRV1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// ✅ CONFIGURACIÓN DE JWT
// ============================================================
// Obtener la configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("Jwt configuration is missing or SecretKey is empty");
}

// Registrar JwtSettings para que esté disponible en el contenedor DI
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ============================================================
// ✅ AUTENTICACIÓN JWT
// ============================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ============================================================
// ✅ HTTP CLIENT PARA USUARIOSSRV4
// ============================================================
builder.Services.AddHttpClient("UsuariosSRV4", client =>
{
    var baseUrl = builder.Configuration["Services:UsuariosSRV4"]
        ?? throw new InvalidOperationException("Services:UsuariosSRV4 no configurado");

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ============================================================
// ✅ SERVICIOS
// ============================================================
builder.Services.AddScoped<IAuthService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("UsuariosSRV4");
    var configuration = sp.GetRequiredService<IConfiguration>();
    var jwtSettingsOptions = sp.GetRequiredService<IOptions<JwtSettings>>();
    var logger = sp.GetRequiredService<ILogger<AuthService>>();

    return new AuthService(httpClient, configuration, jwtSettingsOptions, logger);
});

// ============================================================
// ✅ CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

// ============================================================
// ✅ LOGGING
// ============================================================
builder.Services.AddLogging();

var app = builder.Build();

// ============================================================
// ✅ MIDDLEWARE
// ============================================================
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// ✅ ENDPOINTS DE LOGIN
// ============================================================
app.MapPost("/api/auth/login", async (LoginRequestDto request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return Results.Ok(result);
});

app.MapPost("/api/auth/refresh", async (RefreshTokenRequestDto request, IAuthService authService) =>
{
    var result = await authService.RefreshTokenAsync(request.RefreshToken);
    return Results.Ok(result);
});

app.MapPost("/api/auth/logout", async (string refreshToken, IAuthService authService) =>
{
    var result = await authService.LogoutAsync(refreshToken);
    return Results.Ok(new { success = result });
});

app.MapGet("/api/auth/validate", async (string token, IAuthService authService) =>
{
    var isValid = await authService.ValidateTokenAsync(token);
    return Results.Ok(new { valid = isValid });
});

app.Run();