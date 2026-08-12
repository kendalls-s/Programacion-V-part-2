using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    // Consume el microservicio TipoIdentificacionSRV6 (hosteado en Services:TipoIdentificacionSRV6)
    public class TipoIdentificacionApiClient : ITipoIdentificacionApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TipoIdentificacionApiClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TipoIdentificacionApiClient(HttpClient http, ILogger<TipoIdentificacionApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<TipoIdentificacionDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var envelope = await _http.GetFromJsonAsync<TipoIdentificacionListEnvelope>("api/TipoIdentificacion", JsonOpts, ct);
                return envelope?.Data ?? new List<TipoIdentificacionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la lista de tipos de identificación desde TipoIdentificacionSRV6");
                return new List<TipoIdentificacionDto>();
            }
        }

        public async Task<TipoIdentificacionDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/TipoIdentificacion/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                    return null;

                var envelope = await response.Content.ReadFromJsonAsync<TipoIdentificacionEnvelope>(JsonOpts, ct);
                return envelope?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el tipo de identificación {Id} desde TipoIdentificacionSRV6", id);
                return null;
            }
        }
    }
}
