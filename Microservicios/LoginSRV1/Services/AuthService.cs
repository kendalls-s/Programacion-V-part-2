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

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== INICIO LOGIN ===");
                _logger.LogInformation($"Email: {request.Email}");

                // ✅ SIEMPRE validar contra UsuariosSRV4
                var requestData = new
                {
                    email = request.Email,
                    password = request.Password,
                    tipo = request.Tipo ?? ""
                };

                _logger.LogInformation($"Enviando a UsuariosSRV4: {JsonSerializer.Serialize(requestData)}");

                var response = await _httpClient.PostAsJsonAsync("api/Usuarios/validar-credenciales", requestData);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Respuesta de UsuariosSRV4: {responseContent}");

                // ✅ SI LA RESPUESTA NO ES EXITOSA (400, 404, 500, etc.)
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error en UsuariosSRV4: {response.StatusCode} - {responseContent}");

                    // ✅ Intentar parsear el mensaje de error de UsuariosSRV4
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (errorObj != null && errorObj.TryGetValue("message", out var msg))
                        {
                            _logger.LogWarning($"Error de UsuariosSRV4: {msg}");

                            // ✅ DEVOLVER EL MENSAJE EXACTO DE USUARIOSSRV4
                            return new LoginResponseDto
                            {
                                Success = false,
                                Message = msg
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al parsear respuesta de UsuariosSRV4");
                    }

                    // ✅ Si contiene "bloqueado", devolver mensaje de bloqueo
                    if (responseContent.Contains("bloqueado", StringComparison.OrdinalIgnoreCase))
                    {
                        return new LoginResponseDto
                        {
                            Success = false,
                            Message = "Usuario bloqueado por intentos fallidos. Contacte al administrador."
                        };
                    }

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // ✅ RESPUESTA EXITOSA - Parsear el objeto
                var userResponse = JsonSerializer.Deserialize<ValidarCredencialesResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userResponse == null)
                {
                    _logger.LogWarning("Usuario no encontrado");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                _logger.LogInformation($"Usuario encontrado: ID={userResponse.Id}, Email={userResponse.Email}, Tipo={userResponse.TipoUsuario}");

                // ✅ Verificar bloqueo (por si acaso)
                if (userResponse.Bloqueado)
                {
                    _logger.LogWarning($"Usuario bloqueado: {userResponse.Email}");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario bloqueado por intentos fallidos. Contacte al administrador."
                    };
                }

                // ✅ Verificar activo
                if (!userResponse.Activo)
                {
                    _logger.LogWarning($"Usuario inactivo: {userResponse.Email}");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                // ✅ Crear objeto UserInfoDto
                var user = new UserInfoDto
                {
                    Id = userResponse.Id,
                    Email = userResponse.Email,
                    NombreCompleto = userResponse.NombreCompleto,
                    TipoUsuario = userResponse.TipoUsuario,
                    Activo = userResponse.Activo,
                    TipoUsuarioId = userResponse.TipoUsuarioId,
                    RolId = userResponse.RolId
                };

                // ✅ Generar tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // ✅ Guardar refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UsuarioId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _authDb.RefreshTokens.Add(refreshTokenEntity);
                await _authDb.SaveChangesAsync();

                _logger.LogInformation($"Login exitoso para: {user.Email}");

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en LoginAsync: {ex.Message}");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error al autenticar: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _authDb.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false);

                if (storedToken == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Refresh token inválido"
                    };
                }

                if (storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Refresh token expirado"
                    };
                }

                var response = await _httpClient.GetAsync($"api/Usuarios/{storedToken.UsuarioId}");

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();

                if (user == null || !user.Activo)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                storedToken.IsRevoked = true;
                await _authDb.SaveChangesAsync();

                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                var newRefreshTokenEntity = new RefreshToken
                {
                    UsuarioId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _authDb.RefreshTokens.Add(newRefreshTokenEntity);
                await _authDb.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Token renovado exitosamente",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en RefreshTokenAsync: {ex.Message}");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error al renovar token: {ex.Message}"
                };
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

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

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
                return principal != null;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateAccessToken(UserInfoDto user)
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
                expires: DateTime.UtcNow.AddMinutes(5),
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