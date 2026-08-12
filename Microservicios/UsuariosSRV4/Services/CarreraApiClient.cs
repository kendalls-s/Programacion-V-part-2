using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    // Consume el microservicio SRV3_Carreras (hosteado en Services:CarrerasSRV3)
    public class CarreraApiClient : ICarreraApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CarreraApiClient> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CarreraApiClient(HttpClient http, ILogger<CarreraApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<CarreraDto>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                var envelope = await _http.GetFromJsonAsync<CarreraListEnvelope>("api/Carrera", JsonOpts, ct);
                return envelope?.Data ?? new List<CarreraDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la lista de carreras desde SRV3_Carreras");
                return new List<CarreraDto>();
            }
        }

        public async Task<CarreraDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/Carrera/{id}", ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SRV3_Carreras respondió {Status} al consultar la carrera {Id}", response.StatusCode, id);
                    return null;
                }

                var envelope = await response.Content.ReadFromJsonAsync<CarreraEnvelope>(JsonOpts, ct);
                return envelope?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener la carrera {Id} desde SRV3_Carreras", id);
                return null;
            }
        }
    }
}
