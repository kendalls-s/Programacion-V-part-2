using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    // Consume el microservicio TiposUsuarioSRV5 (hosteado en Services:TiposUsuarioSRV5)
    // Este servicio responde la lista "pelada" (sin sobre codigo/mensaje/data).
    public class TipoUsuarioApiClient : ITipoUsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TipoUsuarioApiClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TipoUsuarioApiClient(HttpClient http, ILogger<TipoUsuarioApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<TipoUsuarioDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<TipoUsuarioDto>>("api/TipoUsuario", JsonOpts, ct);
                return result ?? new List<TipoUsuarioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la lista de tipos de usuario desde TiposUsuarioSRV5");
                return new List<TipoUsuarioDto>();
            }
        }

        public async Task<TipoUsuarioDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/TipoUsuario/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<TipoUsuarioDto>(JsonOpts, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el tipo de usuario {Id} desde TiposUsuarioSRV5", id);
                return null;
            }
        }
    }
}
