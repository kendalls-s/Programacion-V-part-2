using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SRV2_Instituciones.Services
{
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

        public async Task RegistrarAsync(
            string usuario,
            string accion,
            string detalleJson)
        {
            try
            {
                var bitacoraUrl = _configuration["Services:Bitacora"];

                await _httpClient.PostAsJsonAsync(
                    bitacoraUrl,
                    new
                    {
                        Usuario = usuario,
                        Accion = accion,
                        DetalleJson = detalleJson,
                        EsError = false
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar en bitacora: {ex.Message}");
            }
        }
    }
}