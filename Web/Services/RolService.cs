using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class RolService : IRolService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CrearCliente()
        {
            var cliente = _httpClientFactory.CreateClient("Roles");

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

        public async Task<List<Rol>> GetAllAsync()
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync("api/Rol");

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<Rol>();
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RolApiResponse<List<Rol>>>();

            return resultado?.Data ?? new List<Rol>();
        }

        public async Task<Rol?> GetByIdAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.GetAsync($"api/Rol/{id}");

            if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RolApiResponse<Rol>>();

            return resultado?.Data;
        }

        public async Task<bool> CreateAsync(RolRequest request)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.PostAsJsonAsync("api/Rol", request);

            return respuesta.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, RolRequest request)
        {
            var cliente = CrearCliente();

            var respuesta =
                await cliente.PutAsJsonAsync($"api/Rol/{id}", request);

            return respuesta.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cliente = CrearCliente();

            var respuesta = await cliente.DeleteAsync($"api/Rol/{id}");

            return respuesta.IsSuccessStatusCode;
        }
    }
}