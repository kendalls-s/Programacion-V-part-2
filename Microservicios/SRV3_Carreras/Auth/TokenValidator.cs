using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace SRV3_Carreras.Auth;

public class TokenValidator : ITokenValidator
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenValidator> _logger;

    public TokenValidator(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TokenValidator> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("No se recibió un token para validar.");
            return false;
        }

        string? baseUrl = _configuration["Services:LoginSRV1"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogError("No existe la configuración Services:LoginSRV1.");
            return false;
        }

        try
        {
            string url = $"{baseUrl.TrimEnd('/')}/api/auth/validate?token={Uri.EscapeDataString(token.Trim())}";

            _logger.LogInformation("Validando token en: {Url}", url);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            string contenido = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("VALIDATE STATUS: {StatusCode} {Status}",
                (int)response.StatusCode, response.StatusCode);
            _logger.LogInformation("VALIDATE RESPONSE: {Contenido}", contenido);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No se pudo conectar con LoginSRV1: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el token: {Message}", ex.Message);
            return false;
        }
    }
}