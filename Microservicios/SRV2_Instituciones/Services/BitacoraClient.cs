using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SRV2_Instituciones.Services;

public class BitacoraClient : IBitacoraClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public BitacoraClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> RegistrarAsync(
        string token,
        string usuario,
        string accion,
        string detalleJson,
        bool esError = false)
    {
        string? bitacoraUrl =
            _configuration["Services:Bitacora"];

        if (string.IsNullOrWhiteSpace(bitacoraUrl))
        {
            Console.WriteLine(
                "No existe Services:Bitacora en appsettings.json.");

            return false;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine(
                "No se recibió el token para registrar la bitácora.");

            return false;
        }

        try
        {
            using HttpRequestMessage request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    bitacoraUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token.Trim());

            request.Content = JsonContent.Create(new
            {
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalleJson,
                EsError = esError
            });

            using HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            string contenido =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"BITÁCORA STATUS: {(int)response.StatusCode} " +
                $"{response.StatusCode}");

            Console.WriteLine(
                $"BITÁCORA RESPONSE: {contenido}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"ERROR AL REGISTRAR BITÁCORA: {ex.Message}");

            return false;
        }
    }
}