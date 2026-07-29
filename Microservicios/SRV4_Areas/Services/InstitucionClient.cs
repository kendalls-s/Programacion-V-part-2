using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SRV4_Areas.Services;

public class InstitucionClient : IInstitucionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InstitucionClient> _logger;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

    public InstitucionClient(
        HttpClient httpClient,
        ILogger<InstitucionClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // ==========================================
    // OBTENER TODAS LAS INSTITUCIONES
    // ==========================================
    public async Task<List<InstitucionDto>>
        GetAllInstituciones()
    {
        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    "api/Institucion/");

            string contenido =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error al obtener instituciones. " +
                    "Status: {Status}. Respuesta: {Respuesta}",
                    response.StatusCode,
                    contenido);

                return new List<InstitucionDto>();
            }

            InstitucionesResponse? resultado =
                JsonSerializer.Deserialize<InstitucionesResponse>(
                    contenido,
                    _jsonOptions);

            return resultado?.Data
                ?? new List<InstitucionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener las instituciones");

            return new List<InstitucionDto>();
        }
    }

    // ==========================================
    // OBTENER INSTITUCIÓN POR ID
    // ==========================================
    public async Task<InstitucionDto?>
        GetInstitucionById(
            int id,
            string token)
    {
        try
        {
            using HttpRequestMessage request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Institucion/{id}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string contenido =
                await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Consulta de institución {Id}. " +
                "URL: {Url}. Status: {Status}. Respuesta: {Respuesta}",
                id,
                request.RequestUri,
                response.StatusCode,
                contenido);

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error al obtener institución {Id}. " +
                    "Status: {Status}. Respuesta: {Respuesta}",
                    id,
                    response.StatusCode,
                    contenido);

                return null;
            }

            InstitucionResponse? resultado =
                JsonSerializer.Deserialize<InstitucionResponse>(
                    contenido,
                    _jsonOptions);

            return resultado?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener institución con ID {Id}",
                id);

            return null;
        }
    }

    // ==========================================
    // VALIDAR QUE LA INSTITUCIÓN EXISTA
    // ==========================================
    public async Task<bool>
        ValidateInstitucionExists(
            int id,
            string token)
    {
        InstitucionDto? institucion =
            await GetInstitucionById(
                id,
                token);

        return institucion != null;
    }
}