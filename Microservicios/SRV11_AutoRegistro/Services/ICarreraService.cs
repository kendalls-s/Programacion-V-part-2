using System.Net.Http.Json;
using System.Text.Json;

namespace SRV11_AutoRegistro.Services
{
    public interface ICarreraService
    {
        Task<CarreraDto?> GetById(int id);

        Task<List<CarreraDto>> GetAll();
    }

    public class CarreraService : ICarreraService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public CarreraService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<CarreraDto?> GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var carreraUrl =
                _configuration["Services:Carrera"];

            if (string.IsNullOrWhiteSpace(carreraUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Carrera.");

                return null;
            }

            try
            {
                var url =
                    $"{carreraUrl.TrimEnd('/')}/{id}";

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"CarreraService.GetById respondió " +
                        $"{(int)response.StatusCode}.");

                    return null;
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<CarreraDto>
                    >(JsonOptions);

                return resultado?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en CarreraService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"CarreraService.GetById: {ex.Message}");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en CarreraService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
        }

        public async Task<List<CarreraDto>> GetAll()
        {
            var carreraUrl =
                _configuration["Services:Carrera"];

            if (string.IsNullOrWhiteSpace(carreraUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Carrera.");

                return new List<CarreraDto>();
            }

            try
            {
                var url =
                    carreraUrl.TrimEnd('/');

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"CarreraService.GetAll respondió " +
                        $"{(int)response.StatusCode}.");

                    return new List<CarreraDto>();
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<List<CarreraDto>>
                    >(JsonOptions);

                return resultado?.Data
                    ?? new List<CarreraDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en CarreraService.GetAll: " +
                    $"{ex.Message}");

                return new List<CarreraDto>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"CarreraService.GetAll: {ex.Message}");

                return new List<CarreraDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en CarreraService.GetAll: " +
                    $"{ex.Message}");

                return new List<CarreraDto>();
            }
        }
    }

    public class CarreraDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int InstitucionId { get; set; }
    }
}