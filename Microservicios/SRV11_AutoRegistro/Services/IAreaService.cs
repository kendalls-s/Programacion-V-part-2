using System.Net.Http.Json;
using System.Text.Json;

namespace SRV11_AutoRegistro.Services
{
    public interface IAreaService
    {
        Task<AreaDto?> GetById(int id);

        Task<List<AreaDto>> GetAll();
    }

    public class AreaService : IAreaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public AreaService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AreaDto?> GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var areaUrl =
                _configuration["Services:Area"];

            if (string.IsNullOrWhiteSpace(areaUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Area.");

                return null;
            }

            try
            {
                var url =
                    $"{areaUrl.TrimEnd('/')}/{id}";

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"AreaService.GetById respondió " +
                        $"{(int)response.StatusCode}.");

                    return null;
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<AreaDto>
                    >(JsonOptions);

                return resultado?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en AreaService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"AreaService.GetById: {ex.Message}");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en AreaService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
        }

        public async Task<List<AreaDto>> GetAll()
        {
            var areaUrl =
                _configuration["Services:Area"];

            if (string.IsNullOrWhiteSpace(areaUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Area.");

                return new List<AreaDto>();
            }

            try
            {
                var url =
                    areaUrl.TrimEnd('/');

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"AreaService.GetAll respondió " +
                        $"{(int)response.StatusCode}.");

                    return new List<AreaDto>();
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<List<AreaDto>>
                    >(JsonOptions);

                return resultado?.Data
                    ?? new List<AreaDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en AreaService.GetAll: " +
                    $"{ex.Message}");

                return new List<AreaDto>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"AreaService.GetAll: {ex.Message}");

                return new List<AreaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en AreaService.GetAll: " +
                    $"{ex.Message}");

                return new List<AreaDto>();
            }
        }
    }

    public class AreaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int InstitucionId { get; set; }
    }
}