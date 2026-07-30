using System.Net.Http.Json;
using System.Text.Json;

namespace SRV11_AutoRegistro.Services
{
    public interface IInstitucionService
    {
        Task<InstitucionDto?> GetById(int id);

        Task<List<InstitucionDto>> GetAll();
    }

    public class InstitucionService : IInstitucionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public InstitucionService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<InstitucionDto?> GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var institucionUrl =
                _configuration["Services:Institucion"];

            if (string.IsNullOrWhiteSpace(institucionUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Institucion.");

                return null;
            }

            try
            {
                var url =
                    $"{institucionUrl.TrimEnd('/')}/{id}";

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"InstitucionService.GetById respondió " +
                        $"{(int)response.StatusCode}.");

                    return null;
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<InstitucionDto>
                    >(JsonOptions);

                return resultado?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en InstitucionService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"InstitucionService.GetById: {ex.Message}");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en InstitucionService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
        }

        public async Task<List<InstitucionDto>> GetAll()
        {
            var institucionUrl =
                _configuration["Services:Institucion"];

            if (string.IsNullOrWhiteSpace(institucionUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:Institucion.");

                return new List<InstitucionDto>();
            }

            try
            {
                var url =
                    institucionUrl.TrimEnd('/');

                using var response =
                    await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"InstitucionService.GetAll respondió " +
                        $"{(int)response.StatusCode}.");

                    return new List<InstitucionDto>();
                }

                var resultado =
                    await response.Content.ReadFromJsonAsync<
                        ApiResponse<List<InstitucionDto>>
                    >(JsonOptions);

                return resultado?.Data
                    ?? new List<InstitucionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en InstitucionService.GetAll: " +
                    $"{ex.Message}");

                return new List<InstitucionDto>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error procesando JSON en " +
                    $"InstitucionService.GetAll: {ex.Message}");

                return new List<InstitucionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en InstitucionService.GetAll: " +
                    $"{ex.Message}");

                return new List<InstitucionDto>();
            }
        }
    }

    public class ApiResponse<T>
    {
        public int Codigo { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public T? Data { get; set; }
    }

    public class InstitucionDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Dominios { get; set; } = string.Empty;
    }
}