using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    // Consume el microservicio SRV4_Areas (hosteado en Services:AreasSRV4)
    public class AreaApiClient : IAreaApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<AreaApiClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AreaApiClient(HttpClient http, ILogger<AreaApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<AreaDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var envelope = await _http.GetFromJsonAsync<AreaListEnvelope>("api/Area", JsonOpts, ct);
                return envelope?.Data ?? new List<AreaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la lista de áreas desde SRV4_Areas");
                return new List<AreaDto>();
            }
        }

        public async Task<AreaDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/Area/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SRV4_Areas respondió {Status} al consultar el área {Id}", response.StatusCode, id);
                    return null;
                }

                var envelope = await response.Content.ReadFromJsonAsync<AreaEnvelope>(JsonOpts, ct);
                return envelope?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el área {Id} desde SRV4_Areas", id);
                return null;
            }
        }
    }
}
