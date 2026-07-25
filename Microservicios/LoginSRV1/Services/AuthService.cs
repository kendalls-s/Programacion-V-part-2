using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoginSRV1.Data;
using LoginSRV1.DTOs;
using LoginSRV1.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginSRV1.Services
{
    public class AuthService : IAuthService
    {
        private const string MensajeCredencialesInvalidas = "Usuario y/o contraseña incorrectos";
        private const string MensajeNoAutorizado = "No autorizado";

        private readonly AuthDbContext _authDb;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AuthDbContext authDb,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _authDb = authDb;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        // Minutos de validez del access token (JWT). Configurable, default 5 minutos.
        private int AccessTokenExpirationMinutes =>
            _configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 5;

        // Minutos de validez del refresh token. Configurable, debe ser mayor al del access token.
        private int RefreshTokenExpirationMinutes
        {
            get
            {
                var configured = _configuration.GetValue<int?>("Jwt:RefreshTokenExpirationMinutes") ?? 60;
                // Garantiza que el refresh token siempre viva más que el access token.
                return configured > AccessTokenExpirationMinutes
                    ? configured
                    : AccessTokenExpirationMinutes + 1;
            }
        }

        public async Task<AuthOperationResult<LoginSuccessResponseDto>> LoginAsync(string? usuario, string? password, string? tipo)
        {
            // ✅ Validación: todos los datos son requeridos y no pueden ser nulos ni blancos
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(tipo))
            {
                return AuthOperationResult<LoginSuccessResponseDto>.Fail(
                    "Los campos usuario, password y tipo son requeridos y no pueden estar vacíos.",
                    AuthErrorType.Validation);
            }

            try
            {
                _logger.LogInformation("=== INICIO LOGIN === Usuario: {Usuario}", usuario);

                var requestData = new
                {
                    email = usuario,
                    password = password,
                    tipo = tipo
                };

                var response = await _httpClient.PostAsJsonAsync("api/Usuarios/validar-credenciales", requestData);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Credenciales inválidas para el usuario {Usuario}", usuario);
                    return AuthOperationResult<LoginSuccessResponseDto>.Fail(MensajeCredencialesInvalidas, AuthErrorType.Unauthorized);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error en UsuariosSRV4: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return AuthOperationResult<LoginSuccessResponseDto>.Fail(MensajeCredencialesInvalidas, AuthErrorType.Unauthorized);
                }

                var userResponse = await response.Content.ReadFromJsonAsync<ValidarCredencialesResponse>();

                if (userResponse == null || userResponse.Bloqueado || !userResponse.Activo)
                {
                    return AuthOperationResult<LoginSuccessResponseDto>.Fail(MensajeCredencialesInvalidas, AuthErrorType.Unauthorized);
                }

                // Verificación explícita de que el tipo de usuario coincide con el indicado
                if (!string.Equals(userResponse.TipoUsuario, tipo, StringComparison.OrdinalIgnoreCase))
                {
                    return AuthOperationResult<LoginSuccessResponseDto>.Fail(MensajeCredencialesInvalidas, AuthErrorType.Unauthorized);
                }

                var user = new UserInfoDto
                {
                    Id = userResponse.Id,
                    Email = userResponse.Email,
                    NombreCompleto = userResponse.NombreCompleto,
                    TipoUsuario = userResponse.TipoUsuario,
                    Activo = userResponse.Activo,
                    TipoUsuarioId = userResponse.TipoUsuarioId,
                    RolId = userResponse.RolId,
                    Institutions = userResponse.Institutions
                };

                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
                var accessToken = GenerateAccessToken(user, expiresAt);
                var refreshToken = GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    UsuarioId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(RefreshTokenExpirationMinutes).UtcDateTime,
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _authDb.RefreshTokens.Add(refreshTokenEntity);
                await _authDb.SaveChangesAsync();

                var result = new LoginSuccessResponseDto
                {
                    ExpiresIn = expiresAt,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UsuarioId = user.Id,
                    Institutions = user.Institutions ?? new List<InstitutionDto>(),
                    NombreCompleto = user.NombreCompleto,
                    TipoUsuario = user.TipoUsuario
                };

                return AuthOperationResult<LoginSuccessResponseDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoginAsync");
                return AuthOperationResult<LoginSuccessResponseDto>.Fail(MensajeCredencialesInvalidas, AuthErrorType.Unauthorized);
            }
        }

        public async Task<AuthOperationResult<RefreshResponseDto>> RefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return AuthOperationResult<RefreshResponseDto>.Fail(MensajeNoAutorizado, AuthErrorType.Unauthorized);
            }

            try
            {
                var storedToken = await _authDb.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false);

                if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    return AuthOperationResult<RefreshResponseDto>.Fail(MensajeNoAutorizado, AuthErrorType.Unauthorized);
                }

                var response = await _httpClient.GetAsync($"api/Usuarios/{storedToken.UsuarioId}");

                if (!response.IsSuccessStatusCode)
                {
                    return AuthOperationResult<RefreshResponseDto>.Fail(MensajeNoAutorizado, AuthErrorType.Unauthorized);
                }

                var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();

                if (user == null || !user.Activo)
                {
                    return AuthOperationResult<RefreshResponseDto>.Fail(MensajeNoAutorizado, AuthErrorType.Unauthorized);
                }

                // Revocar el token anterior (rotación de refresh tokens)
                storedToken.IsRevoked = true;

                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(AccessTokenExpirationMinutes);
                var newAccessToken = GenerateAccessToken(user, expiresAt);
                var newRefreshToken = GenerateRefreshToken();

                var newRefreshTokenEntity = new RefreshToken
                {
                    UsuarioId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(RefreshTokenExpirationMinutes).UtcDateTime,
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _authDb.RefreshTokens.Add(newRefreshTokenEntity);
                await _authDb.SaveChangesAsync();

                var result = new RefreshResponseDto
                {
                    ExpiresIn = expiresAt,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };

                return AuthOperationResult<RefreshResponseDto>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RefreshTokenAsync");
                return AuthOperationResult<RefreshResponseDto>.Fail(MensajeNoAutorizado, AuthErrorType.Unauthorized);
            }
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _authDb.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _authDb.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> ValidateTokenAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult(false);
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "TuSuperSecretKeyLarga123456789012345678901234567890");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "CUC",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "CUCApp",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return Task.FromResult(principal != null);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private string GenerateAccessToken(UserInfoDto user, DateTimeOffset expiresAt)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? "TuSuperSecretKeyLarga123456789012345678901234567890");
            var issuer = _configuration["Jwt:Issuer"] ?? "CUC";
            var audience = _configuration["Jwt:Audience"] ?? "CUCApp";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NombreCompleto),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.TipoUsuario ?? "Usuario"),
                new Claim("TipoUsuarioId", user.TipoUsuarioId?.ToString() ?? ""),
                new Claim("RolId", user.RolId?.ToString() ?? "")
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
