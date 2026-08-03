using System.Text;
using System.Text.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class LoginService : ILoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginService> _logger;

        public LoginService(IHttpClientFactory httpClientFactory, ILogger<LoginService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Login");

                var loginData = new
                {
                    email = request.Email,
                    password = request.Password,
                    tipo = request.Tipo ?? ""
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/auth/login", content);

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login fallido: {StatusCode} - {Response}", response.StatusCode, responseJson);

                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                        {
                            return new LoginResponse
                            {
                                Success = false,
                                Message = errorResponse.Message
                            };
                        }
                    }
                    catch { }

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Usuario y/o contraseña incorrectos."
                    };
                }

                var result = JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Error al procesar respuesta del servidor"
                    };
                }

                if (!result.Success && string.IsNullOrEmpty(result.Message))
                {
                    result.Message = "Usuario y/o contraseña incorrectos.";
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con el microservicio");
                return new LoginResponse
                {
                    Success = false,
                    Message = "No se pudo conectar con el servidor de autenticación"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoginAsync");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Error de conexión: " + ex.Message
                };
            }
        }

        // ✅ IMPLEMENTACIÓN DEL MÉTODO RefreshTokenAsync
        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Login");

                var refreshData = new { refreshToken = refreshToken };
                var json = JsonSerializer.Serialize(refreshData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/auth/refresh-token", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Refresh token fallido: {StatusCode}", response.StatusCode);
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Error al renovar el token"
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RefreshTokenResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Error al procesar respuesta del servidor"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión en RefreshTokenAsync");
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "No se pudo conectar con el servidor de autenticación"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RefreshTokenAsync");
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Error al renovar el token: " + ex.Message
                };
            }
        }
    }
}