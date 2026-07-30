using System.Text.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class TipoIdentificacionService : ITipoIdentificacionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TipoIdentificacionService> _logger;

        public TipoIdentificacionService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TipoIdentificacionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ✅ GET ALL
        public async Task<List<TipoIdentificacion>> GetAllAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacion");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("api/TipoIdentificacion");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al obtener tipos de identificación: {response.StatusCode}");
                    return new List<TipoIdentificacion>();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta de TipoIdentificacion: {json}");

                // Intentar deserializar como objeto con "data"
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TipoIdentificacion>>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && result.Data != null)
                    {
                        return result.Data;
                    }
                }
                catch { }

                // Intentar deserializar directamente como lista
                try
                {
                    var list = JsonSerializer.Deserialize<List<TipoIdentificacion>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (list != null)
                    {
                        return list;
                    }
                }
                catch { }

                return new List<TipoIdentificacion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllAsync de TipoIdentificacion");
                return new List<TipoIdentificacion>();
            }
        }

        // ✅ GET BY ID
        public async Task<TipoIdentificacion?> GetByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacion");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"api/TipoIdentificacion/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al obtener tipo de identificación {id}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta de TipoIdentificacion/{id}: {json}");

                // Intentar deserializar como objeto con "data"
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TipoIdentificacion>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && result.Data != null)
                    {
                        return result.Data;
                    }
                }
                catch { }

                // Intentar deserializar directamente como objeto
                try
                {
                    var item = JsonSerializer.Deserialize<TipoIdentificacion>(json, new JsonSerializerOptions
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
                _logger.LogError(ex, $"Error en GetByIdAsync de TipoIdentificacion: {id}");
                return null;
            }
        }

        // ✅ CREATE
        public async Task<TipoIdentificacion?> CreateAsync(TipoIdentificacion tipo)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacion");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(new { nombre = tipo.Nombre });
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/TipoIdentificacion/", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al crear tipo de identificación: {response.StatusCode}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta de creación: {responseJson}");

                var result = JsonSerializer.Deserialize<ApiResponse<TipoIdentificacion>>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync de TipoIdentificacion");
                return null;
            }
        }

        // ✅ UPDATE
        public async Task<bool> UpdateAsync(TipoIdentificacion tipo)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacion");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(new { nombre = tipo.Nombre });
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/TipoIdentificacion/{tipo.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al actualizar tipo de identificación {tipo.Id}: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en UpdateAsync de TipoIdentificacion: {tipo.Id}");
                return false;
            }
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacion");

                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync($"api/TipoIdentificacion/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Error al eliminar tipo de identificación {id}: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en DeleteAsync de TipoIdentificacion: {id}");
                return false;
            }
        }
    }

    public class ApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}