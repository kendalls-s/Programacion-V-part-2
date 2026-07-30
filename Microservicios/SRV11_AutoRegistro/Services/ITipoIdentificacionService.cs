using System.Net.Http.Json;
using System.Text.Json;

namespace SRV11_AutoRegistro.Services
{
    public interface ITipoIdentificacionService
    {
        Task<TipoIdentificacionDto?> GetById(int id);

        Task<List<TipoIdentificacionDto>> GetAll();
    }

    public class TipoIdentificacionService
        : ITipoIdentificacionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public TipoIdentificacionService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<TipoIdentificacionDto?> GetById(
            int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var tipoIdentificacionUrl =
                _configuration[
                    "Services:TipoIdentificacion"];

            if (string.IsNullOrWhiteSpace(
                tipoIdentificacionUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:TipoIdentificacion.");

                return null;
            }

            try
            {
                var url =
                    $"{tipoIdentificacionUrl.TrimEnd('/')}/{id}";

                Console.WriteLine(
                    $"Consultando TipoIdentificacion: {url}");

                using var response =
                    await _httpClient.GetAsync(url);

                Console.WriteLine(
                    $"TipoIdentificacion respondió: {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content
                    .ReadFromJsonAsync<TipoIdentificacionDto>(
                        JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en TipoIdentificacionService.GetById: {ex.Message}");

                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error JSON en TipoIdentificacionService.GetById: {ex.Message}");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en TipoIdentificacionService.GetById: {ex.Message}");

                return null;
            }
        }

        public async Task<List<TipoIdentificacionDto>>
            GetAll()
        {
            var tipoIdentificacionUrl =
                _configuration[
                    "Services:TipoIdentificacion"];

            if (string.IsNullOrWhiteSpace(
                tipoIdentificacionUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:TipoIdentificacion.");

                return new List<TipoIdentificacionDto>();
            }

            try
            {
                var url =
                    tipoIdentificacionUrl.TrimEnd('/');

                Console.WriteLine(
                    $"Consultando tipos de identificación: {url}");

                using var response =
                    await _httpClient.GetAsync(url);

                Console.WriteLine(
                    $"TipoIdentificacion respondió: {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<TipoIdentificacionDto>();
                }

                return await response.Content
                    .ReadFromJsonAsync<List<TipoIdentificacionDto>>(
                        JsonOptions)
                    ?? new List<TipoIdentificacionDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en TipoIdentificacionService.GetAll: {ex.Message}");

                return new List<TipoIdentificacionDto>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error JSON en TipoIdentificacionService.GetAll: {ex.Message}");

                return new List<TipoIdentificacionDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en TipoIdentificacionService.GetAll: {ex.Message}");

                return new List<TipoIdentificacionDto>();
            }
        }
    }

    public class TipoIdentificacionDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}