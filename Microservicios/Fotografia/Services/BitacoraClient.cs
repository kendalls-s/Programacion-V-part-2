using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SRV13_Fotografia.Services
{
    // Cliente HTTP que envia los movimientos (agregar / editar / eliminar)
    // al microservicio de Bitacora ya hosteado. La URL se lee de
    // appsettings.json -> Services:Bitacora
    public class BitacoraClient : IBitacoraClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BitacoraClient(HttpClient httpClient, IConfiguration configuration)
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
            var bitacoraUrl = _configuration["Services:Bitacora"];

            if (string.IsNullOrWhiteSpace(bitacoraUrl))
            {
                Console.WriteLine("No existe Services:Bitacora en appsettings.json.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("No se recibio el token para registrar la bitacora.");
                return false;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Trim());

                request.Content = JsonContent.Create(new
                {
                    Usuario = usuario,
                    Accion = accion,
                    DetalleJson = detalleJson,
                    EsError = esError
                });

                var response = await _httpClient.SendAsync(request);
                var contenido = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"BITACORA STATUS: {response.StatusCode}");
                Console.WriteLine($"BITACORA RESPONSE: {contenido}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR AL REGISTRAR BITACORA: {ex.Message}");
                return false;
            }
        }
    }
}
