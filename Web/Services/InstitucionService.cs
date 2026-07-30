// InstitucionService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class InstitucionService : IInstitucionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstitucionService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CrearCliente()
        {
            var cliente = _httpClientFactory.CreateClient("Instituciones");

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

        public async Task<List<Institucion>> GetAllAsync()
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync("api/Institucion");

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<Institucion>();
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<ApiResponse<List<Institucion>>>();

            return resultado?.Data ?? new List<Institucion>();
        }

        public async Task<Institucion?> GetByIdAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync($"api/Institucion/{id}");

            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<ApiResponse<Institucion>>();

            return resultado?.Data;
        }

        public async Task<(bool success, string message, int? id)> CreateAsync(InstitucionCreateRequest request)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.PostAsJsonAsync("api/Institucion", request);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al crear la institución", null);
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<ApiResponse<Institucion>>();

            return (true, resultado?.Mensaje ?? "Institución creada exitosamente", resultado?.Data?.ID);
        }

        public async Task<(bool success, string message)> UpdateAsync(InstitucionUpdateRequest request)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.PutAsJsonAsync($"api/Institucion/{request.ID}", request);

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al actualizar la institución");
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<ApiResponse<object>>();

            return (true, resultado?.Mensaje ?? "Institución actualizada exitosamente");
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.DeleteAsync($"api/Institucion/{id}");

            if (!respuesta.IsSuccessStatusCode)
            {
                var errorContent = await respuesta.Content.ReadAsStringAsync();
                return (false, errorContent ?? "Error al eliminar la institución");
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<ApiResponse<object>>();

            return (true, resultado?.Mensaje ?? "Institución eliminada exitosamente");
        }
    }
}