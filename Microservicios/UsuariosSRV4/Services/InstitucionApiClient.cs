using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    // Consume el microservicio SRV2_Instituciones (hosteado en Services:InstitucionesSRV2)
    public class InstitucionApiClient : IInstitucionApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<InstitucionApiClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public InstitucionApiClient(HttpClient http, ILogger<InstitucionApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<InstitucionDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var envelope = await _http.GetFromJsonAsync<InstitucionListEnvelope>("api/Institucion", JsonOpts, ct);
                return envelope?.Data ?? new List<InstitucionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la lista de instituciones desde SRV2_Instituciones");
                return new List<InstitucionDto>();
            }
        }

        public async Task<InstitucionDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/Institucion/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SRV2_Instituciones respondió {Status} al consultar la institución {Id}", response.StatusCode, id);
                    return null;
                }

                var envelope = await response.Content.ReadFromJsonAsync<InstitucionEnvelope>(JsonOpts, ct);
                return envelope?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la institución {Id} desde SRV2_Instituciones", id);
                return null;
            }
        }
    }
}
