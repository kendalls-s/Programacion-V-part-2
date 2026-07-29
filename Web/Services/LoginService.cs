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
                _logger.LogInformation($"=== LoginService.LoginAsync ===");
                _logger.LogInformation($"Email: {request.Email}");
                _logger.LogInformation($"Password: {request.Password}");
                _logger.LogInformation($"Tipo: {request.Tipo}");

                // ✅ VALIDAR QUE LOS DATOS NO ESTÉN VACÍOS
                if (string.IsNullOrEmpty(request.Email))
                {
                    _logger.LogWarning("Email vacío en LoginService");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "El email es requerido"
                    };
                }

                if (string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Password vacío en LoginService");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "La contraseña es requerida"
                    };
                }

                var client = _httpClientFactory.CreateClient("Login");

                var loginData = new
                {
                    email = request.Email,
                    password = request.Password,
                    tipo = request.Tipo ?? ""
                };

                var json = JsonSerializer.Serialize(loginData);
                _logger.LogInformation($"JSON enviado a LoginSRV1: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/auth/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Respuesta de LoginSRV1: {responseJson}");
                _logger.LogInformation($"Status Code: {response.StatusCode}");

                // ✅ Intentar deserializar la respuesta
                try
                {
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al deserializar LoginResponse");
                }

                // ✅ Si no se pudo deserializar, intentar como mensaje de error simple
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (errorObj != null && errorObj.TryGetValue("message", out var msg))
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = msg
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al deserializar mensaje de error");
                }

                return new LoginResponse
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LoginAsync");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error de conexión: {ex.Message}"
                };
            }
        }
    }
}