using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SRV3_Carreras.Services;

public class BitacoraClient : IBitacoraClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BitacoraClient> _logger;

    public BitacoraClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BitacoraClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        string token,
        string usuario,
        string accion,
        string detalleJson,
        bool esError = false)
    {
        string? bitacoraUrl = _configuration["Services:Bitacora"];

        if (string.IsNullOrWhiteSpace(bitacoraUrl))
        {
            _logger.LogError("No existe Services:Bitacora en appsettings.json.");
            return;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("No se recibió el token para registrar la bitácora.");
            return;
        }

        try
        {
            using HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                bitacoraUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.Trim());

            request.Content = JsonContent.Create(new
            {
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalleJson,
                EsError = esError
            });

            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            string contenido = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("BITÁCORA STATUS: {StatusCode} {Status}",
                (int)response.StatusCode, response.StatusCode);
            _logger.LogInformation("BITÁCORA RESPONSE: {Contenido}", contenido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR AL REGISTRAR BITÁCORA: {Message}", ex.Message);
        }
    }
}