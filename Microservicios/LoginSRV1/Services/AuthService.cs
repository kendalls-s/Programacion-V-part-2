using BCrypt.Net;
using LoginSRV1.Config;
using LoginSRV1.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LoginSRV1.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        private readonly string _usuariosUrl;

        public AuthService(
            HttpClient httpClient,
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _usuariosUrl = configuration["Services:UsuariosSRV4"]
                ?? throw new InvalidOperationException("Services:UsuariosSRV4 no configurado");
        }

        // ============================================================
        // ✅ LOGIN
        // ============================================================
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== LOGIN REQUEST ===");
                _logger.LogInformation("Email: {Email}", request.Email);
                _logger.LogInformation("Tipo: {Tipo}", request.Tipo);

                // Obtener usuario del microservicio
                var usuario = await ObtenerUsuarioAsync(request.Email, request.Tipo);

                // ✅ LOGS PARA DEPURAR
                _logger.LogInformation("=== DATOS DEL USUARIO RECIBIDOS ===");
                _logger.LogInformation("Email: {Email}", usuario?.Email);
                _logger.LogInformation("PasswordHash: {Hash}", usuario?.PasswordHash);
                _logger.LogInformation("Longitud del hash: {Length}", usuario?.PasswordHash?.Length ?? 0);
                _logger.LogInformation("¿Empieza con $2a$? {StartsWith}", usuario?.PasswordHash?.StartsWith("$2a$") ?? false);
                _logger.LogInformation("Hash completo: '{Hash}'", usuario?.PasswordHash);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", request.Email);
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario y/o contraseña incorrectos."
                    };
                }

                if (usuario.Bloqueado)
                {
                    _logger.LogWarning("Usuario bloqueado: {Email}", request.Email);
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario bloqueado permanentemente por múltiples intentos fallidos."
                    };
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo: {Email}", request.Email);
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario inactivo. Contacte al administrador."
                    };
                }

                bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);

                if (!passwordValida)
                {
                    await IncrementarIntentosFallidosAsync(request.Email);
                    var intentos = await ObtenerIntentosFallidosAsync(request.Email);
                    _logger.LogWarning("Contraseña incorrecta. Intentos fallidos: {Intentos}", intentos);

                    if (intentos >= 3)
                    {
                        await BloquearUsuarioAsync(request.Email);
                        _logger.LogWarning("Usuario bloqueado permanentemente: {Email}", request.Email);
                        return new LoginResponseDto
                        {
                            Success = false,
                            Message = "Usuario bloqueado permanentemente por múltiples intentos fallidos."
                        };
                    }

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Usuario y/o contraseña incorrectos."
                    };
                }

                await ResetearIntentosFallidosAsync(request.Email);

                var token = GenerateJwtToken(usuario);
                var refreshToken = GenerateRefreshToken();

                await GuardarRefreshTokenAsync(usuario.Id, refreshToken);

                _logger.LogInformation("✅ Login exitoso: {Email}", request.Email);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
                    User = new UserInfoDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        TipoUsuario = usuario.Tipo,
                        TipoUsuarioId = usuario.TipoUsuarioId,
                        RolId = usuario.RolId,
                        Activo = usuario.Activo,
                        Rol = usuario.Rol
                    }
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión con UsuariosSRV4");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Error de conexión con el servidor de usuarios"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en LoginAsync");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        // ============================================================
        // ✅ REFRESH TOKEN
        // ============================================================
        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("=== REFRESH TOKEN REQUEST ===");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Refresh token requerido"
                    };
                }

                var usuario = await ValidarRefreshTokenAsync(refreshToken);

                if (usuario == null)
                {
                    _logger.LogWarning("Refresh token inválido o expirado");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Refresh token inválido o expirado"
                    };
                }

                var newToken = GenerateJwtToken(usuario);
                var newRefreshToken = GenerateRefreshToken();

                await ActualizarRefreshTokenAsync(usuario.Id, newRefreshToken);

                _logger.LogInformation("✅ Token renovado para: {Email}", usuario.Email);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Token renovado exitosamente",
                    AccessToken = newToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
                    User = new UserInfoDto
                    {
                        Id = usuario.Id,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        TipoUsuario = usuario.Tipo,
                        TipoUsuarioId = usuario.TipoUsuarioId,
                        RolId = usuario.RolId,
                        Activo = usuario.Activo,
                        Rol = usuario.Rol
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en RefreshTokenAsync");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        // ============================================================
        // ✅ LOGOUT
        // ============================================================
        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                await EliminarRefreshTokenAsync(refreshToken);
                _logger.LogInformation("✅ Logout exitoso");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en LogoutAsync");
                return false;
            }
        }

        // ============================================================
        // ✅ VALIDATE TOKEN
        // ============================================================
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // ✅ VALIDAR CREDENCIALES - Para otros microservicios
        // ============================================================
        public async Task<ValidarCredencialesResponse?> ValidarCredencialesAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("=== VALIDAR CREDENCIALES ===");
                _logger.LogInformation("Email: {Email}", email);

                var usuario = await ObtenerUsuarioPorEmailAsync(email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", email);
                    return null;
                }

                if (usuario.Bloqueado || !usuario.Activo)
                {
                    _logger.LogWarning("Usuario bloqueado o inactivo: {Email}", email);
                    return null;
                }

                bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

                if (!passwordValida)
                {
                    _logger.LogWarning("Contraseña incorrecta para: {Email}", email);
                    return null;
                }

                return new ValidarCredencialesResponse
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    TipoUsuario = usuario.Tipo,
                    Activo = usuario.Activo,
                    Bloqueado = usuario.Bloqueado,
                    IntentosFallidos = usuario.IntentosFallidos,
                    TipoUsuarioId = usuario.TipoUsuarioId ?? 0,
                    RolId = usuario.RolId ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en ValidarCredencialesAsync");
                return null;
            }
        }

        // ============================================================
        // ✅ REGISTRO - Crea un nuevo usuario
        // ============================================================
        public async Task<RegistroResponseDto> RegistroAsync(RegistroRequestDto request)
        {
            try
            {
                _logger.LogInformation("=== REGISTRO REQUEST ===");
                _logger.LogInformation("Email: {Email}", request.Email);

                // Verificar si el usuario ya existe
                var existe = await VerificarUsuarioExistenteAsync(request.Email);

                if (existe)
                {
                    return new RegistroResponseDto
                    {
                        Success = false,
                        Message = "El correo electrónico ya está registrado"
                    };
                }

                // ✅ Generar hash de la contraseña con BCrypt
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);

                // Crear usuario en UsuariosSRV4
                var usuarioCreado = await CrearUsuarioAsync(new
                {
                    request.Email,
                    PasswordHash = passwordHash,
                    request.NombreCompleto,
                    request.Tipo,
                    request.TipoUsuarioId,
                    request.RolId,
                    Activo = true,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    FechaCreacion = DateTime.UtcNow
                });

                if (!usuarioCreado)
                {
                    return new RegistroResponseDto
                    {
                        Success = false,
                        Message = "Error al crear el usuario"
                    };
                }

                _logger.LogInformation("✅ Usuario registrado: {Email}", request.Email);

                return new RegistroResponseDto
                {
                    Success = true,
                    Message = "Usuario registrado correctamente",
                    Email = request.Email,
                    NombreCompleto = request.NombreCompleto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en RegistroAsync");
                return new RegistroResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        // ============================================================
        // ✅ MÉTODOS PRIVADOS - Comunicación con UsuariosSRV4
        // ============================================================

        private async Task<UsuarioDto?> ObtenerUsuarioAsync(string email, string tipo)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/por-email-tipo?email={Uri.EscapeDataString(email)}&tipo={Uri.EscapeDataString(tipo ?? "")}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UsuarioDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        private async Task<UsuarioDto?> ObtenerUsuarioPorEmailAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/por-email?email={Uri.EscapeDataString(email)}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UsuarioDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> VerificarUsuarioExistenteAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/existe?email={Uri.EscapeDataString(email)}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExisteResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Existe ?? false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CrearUsuarioAsync(object usuarioData)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(usuarioData),
                    Encoding.UTF8,
                    "application/json");

                var url = $"{_usuariosUrl}/api/Usuarios/crear";
                var response = await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<int> ObtenerIntentosFallidosAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/intentos-fallidos?email={Uri.EscapeDataString(email)}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<IntentosResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.IntentosFallidos ?? 0;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task IncrementarIntentosFallidosAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/incrementar-intentos?email={Uri.EscapeDataString(email)}";
                await _httpClient.PostAsync(url, null);
            }
            catch { }
        }

        private async Task ResetearIntentosFallidosAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/resetear-intentos?email={Uri.EscapeDataString(email)}";
                await _httpClient.PostAsync(url, null);
            }
            catch { }
        }

        private async Task BloquearUsuarioAsync(string email)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/bloquear?email={Uri.EscapeDataString(email)}";
                await _httpClient.PostAsync(url, null);
            }
            catch { }
        }

        private async Task GuardarRefreshTokenAsync(int usuarioId, string refreshToken)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { UsuarioId = usuarioId, RefreshToken = refreshToken }),
                    Encoding.UTF8,
                    "application/json");

                var url = $"{_usuariosUrl}/api/Usuarios/guardar-refresh-token";
                await _httpClient.PostAsync(url, content);
            }
            catch { }
        }

        private async Task<UsuarioDto?> ValidarRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/validar-refresh-token?refreshToken={Uri.EscapeDataString(refreshToken)}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UsuarioDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        private async Task ActualizarRefreshTokenAsync(int usuarioId, string refreshToken)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { UsuarioId = usuarioId, RefreshToken = refreshToken }),
                    Encoding.UTF8,
                    "application/json");

                var url = $"{_usuariosUrl}/api/Usuarios/actualizar-refresh-token";
                await _httpClient.PutAsync(url, content);
            }
            catch { }
        }

        private async Task EliminarRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var url = $"{_usuariosUrl}/api/Usuarios/eliminar-refresh-token?refreshToken={Uri.EscapeDataString(refreshToken)}";
                await _httpClient.DeleteAsync(url);
            }
            catch { }
        }

        // ============================================================
        // ✅ MÉTODOS PRIVADOS - JWT
        // ============================================================

        private string GenerateJwtToken(UsuarioDto usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim("Tipo", usuario.Tipo),
                new Claim("TipoUsuarioId", usuario.TipoUsuarioId?.ToString() ?? ""),
                new Claim("RolId", usuario.RolId?.ToString() ?? ""),
                new Claim("Rol", usuario.Rol ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    // ============================================================
    // ✅ DTOs para comunicación con UsuariosSRV4
    // ============================================================

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int? TipoUsuarioId { get; set; }
        public int? RolId { get; set; }
        public string? Rol { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
        public int IntentosFallidos { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public class IntentosResponse
    {
        public int IntentosFallidos { get; set; }
    }

    public class ExisteResponse
    {
        public bool Existe { get; set; }
    }
}