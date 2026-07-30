using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CarreraService> _logger;

        public CarreraService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CarreraService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private HttpClient CrearCliente()
        {
            var cliente = _httpClientFactory.CreateClient("Carreras");

            var token = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                cliente.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return cliente;
        }

        public async Task<List<Models.Carrera>> GetAllAsync()
        {
            try
            {
                var cliente = CrearCliente();
                var respuesta = await cliente.GetAsync("api/Carrera");

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener carreras. Status: {Status}",
                        respuesta.StatusCode);
                    return new List<Models.Carrera>();
                }

                var resultado = await respuesta.Content
                    .ReadFromJsonAsync<ApiResponse<List<Models.Carrera>>>();

                return resultado?.Data ?? new List<Models.Carrera>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carreras");
                return new List<Models.Carrera>();
            }
        }

        public async Task<Models.Carrera?> GetByIdAsync(int id)
        {
            try
            {
                var cliente = CrearCliente();
                var respuesta = await cliente.GetAsync($"api/Carrera/{id}");

                if (!respuesta.IsSuccessStatusCode)
                {
                    if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    _logger.LogWarning("Error al obtener carrera {Id}. Status: {Status}",
                        id, respuesta.StatusCode);
                    return null;
                }

                var resultado = await respuesta.Content
                    .ReadFromJsonAsync<ApiResponse<Models.Carrera>>();

                return resultado?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carrera con ID {Id}", id);
                return null;
            }
        }

        public async Task<(bool success, string message, int? id)> CreateAsync(
            CreateCarreraRequest request)
        {
            try
            {
                var cliente = CrearCliente();
                var respuesta = await cliente.PostAsJsonAsync("api/Carrera", request);

                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear carrera: {Error}", errorContent);
                    return (false, errorContent ?? "Error al crear la carrera", null);
                }

                var resultado = await respuesta.Content
                    .ReadFromJsonAsync<ApiResponse<Models.Carrera>>();

                return (true, resultado?.Message ?? "Carrera creada exitosamente",
                    resultado?.Data?.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear carrera");
                return (false, $"Error al crear la carrera: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> UpdateAsync(
            UpdateCarreraRequest request)
        {
            try
            {
                var cliente = CrearCliente();
                var respuesta = await cliente.PutAsJsonAsync($"api/Carrera/{request.ID}", request);

                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar carrera {Id}: {Error}",
                        request.ID, errorContent);
                    return (false, errorContent ?? "Error al actualizar la carrera");
                }

                var resultado = await respuesta.Content
                    .ReadFromJsonAsync<ApiResponse<object>>();

                return (true, resultado?.Message ?? "Carrera actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar carrera con ID {Id}", request.ID);
                return (false, $"Error al actualizar la carrera: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            try
            {
                var cliente = CrearCliente();
                var respuesta = await cliente.DeleteAsync($"api/Carrera/{id}");

                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar carrera {Id}: {Error}",
                        id, errorContent);
                    return (false, errorContent ?? "Error al eliminar la carrera");
                }

                var resultado = await respuesta.Content
                    .ReadFromJsonAsync<ApiResponse<object>>();

                return (true, resultado?.Message ?? "Carrera eliminada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar carrera con ID {Id}", id);
                return (false, $"Error al eliminar la carrera: {ex.Message}");
            }
        }
    }
}