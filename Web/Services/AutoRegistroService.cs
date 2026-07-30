using System.Net.Http.Json;
using System.Text.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services;

public class AutoRegistroService : IAutoRegistroService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AutoRegistroService> _logger;

    public AutoRegistroService(
        IHttpClientFactory httpClientFactory,
        ILogger<AutoRegistroService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(bool success, string message)> RegistrarAsync(
        RegistroUsuarioRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AutoRegistro");

            var response = await client.PostAsJsonAsync(
                "autoregistro",
                request);

            var contenido = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Respuesta de AutoRegistro. Código: {StatusCode}. Contenido: {Contenido}",
                response.StatusCode,
                contenido);

            if (response.IsSuccessStatusCode)
            {
                return (
                    true,
                    ObtenerMensaje(
                        contenido,
                        "Usuario registrado correctamente. Revise su correo para confirmar la cuenta.")
                );
            }

            return (
                false,
                ObtenerMensaje(
                    contenido,
                    "No se pudo completar el registro.")
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "No se pudo conectar con el microservicio de AutoRegistro.");

            return (
                false,
                "No se pudo conectar con el servicio de AutoRegistro."
            );
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "El servicio de AutoRegistro excedió el tiempo de espera.");

            return (
                false,
                "El servicio tardó demasiado en responder."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al registrar el usuario.");

            return (
                false,
                "Ocurrió un error inesperado durante el registro."
            );
        }
    }

    private static string ObtenerMensaje(
        string contenido,
        string mensajePredeterminado)
    {
        if (string.IsNullOrWhiteSpace(contenido))
        {
            return mensajePredeterminado;
        }

        try
        {
            using var documento = JsonDocument.Parse(contenido);
            var raiz = documento.RootElement;

            if (raiz.TryGetProperty("mensaje", out var mensaje))
            {
                return mensaje.GetString() ?? mensajePredeterminado;
            }

            if (raiz.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? mensajePredeterminado;
            }

            if (raiz.TryGetProperty("error", out var error))
            {
                return error.GetString() ?? mensajePredeterminado;
            }
        }
        catch (JsonException)
        {
            return contenido;
        }

        return mensajePredeterminado;
    }
}