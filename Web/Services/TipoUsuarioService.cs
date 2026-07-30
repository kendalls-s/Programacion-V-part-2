using System.Text;
using System.Text.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class TipoUsuarioService : ITipoUsuarioService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TipoUsuarioService> _logger;

        public TipoUsuarioService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TipoUsuarioService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<TipoUsuario>> GetAllAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TiposUsuario");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("api/TipoUsuario");

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"=== RESPUESTA DE TIPOS USUARIO ===");
                _logger.LogInformation($"Status: {response.StatusCode}");
                _logger.LogInformation($"JSON: {json}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al obtener tipos de usuario: {response.StatusCode}");
                    return new List<TipoUsuario>();
                }

                // ✅ Deserializar usando la estructura de la respuesta
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // ✅ Usar la estructura que devuelve el microservicio
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TipoUsuario>>>(json, options);

                    if (result != null && result.Data != null)
                    {
                        _logger.LogInformation($"✅ Se encontraron {result.Data.Count} tipos de usuario");
                        return result.Data;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al deserializar como ApiResponse");
                }

                // ✅ Si falla, intentar deserializar como lista directamente
                try
                {
                    var list = JsonSerializer.Deserialize<List<TipoUsuario>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (list != null)
                    {
                        _logger.LogInformation($"✅ Se encontraron {list.Count} tipos (lista directa)");
                        return list;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al deserializar como lista");
                }

                return new List<TipoUsuario>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllAsync de TipoUsuario");
                return new List<TipoUsuario>();
            }
        }

        public async Task<TipoUsuario?> GetByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TiposUsuario");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"api/TipoUsuario/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al obtener tipo de usuario {id}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta de TipoUsuario/{id}: {json}");

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TipoUsuario>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && result.Data != null)
                    {
                        return result.Data;
                    }
                }
                catch { }

                try
                {
                    var item = JsonSerializer.Deserialize<TipoUsuario>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (item != null)
                    {
                        return item;
                    }
                }
                catch { }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en GetByIdAsync de TipoUsuario: {id}");
                return null;
            }
        }

        public async Task<TipoUsuario?> CreateAsync(TipoUsuario tipo)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TiposUsuario");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(new { Nombre = tipo.Nombre });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/TipoUsuario/", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al crear tipo de usuario: {response.StatusCode}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta de creación: {responseJson}");

                var result = JsonSerializer.Deserialize<ApiResponse<TipoUsuario>>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync de TipoUsuario");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(TipoUsuario tipo)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TiposUsuario");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(new { Nombre = tipo.Nombre });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/TipoUsuario/{tipo.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al actualizar tipo de usuario {tipo.Id}: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en UpdateAsync de TipoUsuario: {tipo.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TiposUsuario");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync($"api/TipoUsuario/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al eliminar tipo de usuario {id}: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en DeleteAsync de TipoUsuario: {id}");
                return false;
            }
        }
    }
}