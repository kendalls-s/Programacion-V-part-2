using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TiposUsuarioSRV5.Services
{
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

            Console.WriteLine($"=== REGISTRANDO BITÁCORA ===");
            Console.WriteLine($"URL: {bitacoraUrl}");
            Console.WriteLine($"Usuario: {usuario}");
            Console.WriteLine($"Acción: {accion}");
            Console.WriteLine($"Token: {(string.IsNullOrEmpty(token) ? "NO" : "SI")}");
            Console.WriteLine($"Detalle: {detalleJson}");

            if (string.IsNullOrWhiteSpace(bitacoraUrl))
            {
                Console.WriteLine("❌ No existe Services:Bitacora en appsettings.json.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("❌ No se recibió el token para registrar la bitácora.");
                return false;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
                request.Content = JsonContent.Create(new
                {
                    Usuario = usuario,
                    Accion = accion,
                    DetalleJson = detalleJson,
                    EsError = esError
                });

                Console.WriteLine($"📤 Enviando petición a Bitácora...");

                var response = await _httpClient.SendAsync(request);
                var contenido = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"✅ BITÁCORA STATUS: {response.StatusCode}");
                Console.WriteLine($"✅ BITÁCORA RESPONSE: {contenido}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR AL REGISTRAR BITÁCORA: {ex.Message}");
                return false;
            }
        }
    }
}