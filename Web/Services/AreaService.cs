// Services/AreaService.cs (versión mejorada con manejo de respuestas)
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class AreaService : IAreaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AreaService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CrearCliente()
        {
            var cliente = _httpClientFactory.CreateClient("Areas");

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

        public async Task<List<Area>> GetAllAsync()
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync("api/Area");

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<Area>();
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<AreaApiResponse<List<Area>>>();

            return resultado?.Data ?? new List<Area>();
        }

        public async Task<Area?> GetByIdAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync($"api/Area/{id}");

            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<AreaApiResponse<Area>>();

            return resultado?.Data;
        }

        public async Task<(bool success, string message, int? id)> CreateAsync(Area area)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.PostAsJsonAsync("api/Area", area);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al crear el área", null);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<AreaApiResponse<Area>>();

            return (true, resultado?.Mensaje ?? "Área creada exitosamente", resultado?.Data?.ID);
        }

        public async Task<(bool success, string message)> UpdateAsync(int id, Area area)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.PutAsJsonAsync($"api/Area/{id}", area);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al actualizar el área");
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<AreaApiResponse<object>>();

            return (true, resultado?.Mensaje ?? "Área actualizada exitosamente");
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.DeleteAsync($"api/Area/{id}");

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al eliminar el área");
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<AreaApiResponse<object>>();

            return (true, resultado?.Mensaje ?? "Área eliminada exitosamente");
        }
    }
}