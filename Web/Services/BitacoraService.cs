using System.Net.Http.Headers;
using System.Text.Json;
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BitacoraService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<BitacoraRespuestaModel> ObtenerConFiltrosAsync(
            string? token,
            DateTime? fecha,
            string? usuario,
            string? accion,
            int pagina,
            int tamanoPagina,
            bool soloErrores)
        {
            var urlBase =
                _configuration["Services:Bitacora"]
                ?? throw new InvalidOperationException(
                    "No se encontró Services:Bitacora en appsettings.json."
                );

            var parametros = new List<string>
            {
                $"pagina={pagina}",
                $"tamanoPagina={tamanoPagina}",
                $"soloErrores={soloErrores.ToString().ToLowerInvariant()}"
            };

            if (fecha.HasValue)
            {
                var fechaInicio =
                    fecha.Value.Date
                        .ToString("yyyy-MM-dd");

                var fechaFin =
                    fecha.Value.Date
                        .AddDays(1)
                        .AddTicks(-1)
                        .ToString("yyyy-MM-ddTHH:mm:ss");

                parametros.Add(
                    $"fechaInicio={Uri.EscapeDataString(fechaInicio)}"
                );

                parametros.Add(
                    $"fechaFin={Uri.EscapeDataString(fechaFin)}"
                );
            }

            if (!string.IsNullOrWhiteSpace(usuario))
            {
                parametros.Add(
                    $"usuario={Uri.EscapeDataString(usuario.Trim())}"
                );
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                parametros.Add(
                    $"accion={Uri.EscapeDataString(accion.Trim())}"
                );
            }

            var url =
                $"{urlBase.TrimEnd('/')}/bitacora/filtros" +
                $"?{string.Join("&", parametros)}";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );
            }

            using var response =
                await _httpClient.SendAsync(request);

            var contenido =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Bitácora respondió {(int)response.StatusCode}: {contenido}"
                );
            }

            var opciones =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            var resultado =
                JsonSerializer.Deserialize<BitacoraRespuestaModel>(
                    contenido,
                    opciones
                );

            return resultado ??
                   new BitacoraRespuestaModel();
        }
    }
}