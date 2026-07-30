using System.Net.Http.Json;
using System.Text.Json;

namespace SRV11_AutoRegistro.Services
{
    public interface ITipoUsuarioService
    {
        Task<TipoUsuarioDto?> GetById(int id);

        Task<List<TipoUsuarioDto>> GetAll();
    }

    public class TipoUsuarioService : ITipoUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public TipoUsuarioService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<TipoUsuarioDto?> GetById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var tipoUsuarioUrl =
                _configuration["Services:TipoUsuario"];

            if (string.IsNullOrWhiteSpace(tipoUsuarioUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:TipoUsuario.");

                return null;
            }

            try
            {
                var url =
                    $"{tipoUsuarioUrl.TrimEnd('/')}/{id}";

                Console.WriteLine(
                    $"Consultando TipoUsuario: {url}");

                using var response =
                    await _httpClient.GetAsync(url);

                Console.WriteLine(
                    $"TipoUsuario respondió: " +
                    $"{(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content
                    .ReadFromJsonAsync<TipoUsuarioDto>(
                        JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en TipoUsuarioService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error JSON en TipoUsuarioService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en TipoUsuarioService.GetById: " +
                    $"{ex.Message}");

                return null;
            }
        }

        public async Task<List<TipoUsuarioDto>> GetAll()
        {
            var tipoUsuarioUrl =
                _configuration["Services:TipoUsuario"];

            if (string.IsNullOrWhiteSpace(tipoUsuarioUrl))
            {
                Console.WriteLine(
                    "No está configurada Services:TipoUsuario.");

                return new List<TipoUsuarioDto>();
            }

            try
            {
                var url =
                    tipoUsuarioUrl.TrimEnd('/');

                Console.WriteLine(
                    $"Consultando tipos de usuario: {url}");

                using var response =
                    await _httpClient.GetAsync(url);

                Console.WriteLine(
                    $"TipoUsuario respondió: " +
                    $"{(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<TipoUsuarioDto>();
                }

                return await response.Content
                    .ReadFromJsonAsync<List<TipoUsuarioDto>>(
                        JsonOptions)
                    ?? new List<TipoUsuarioDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Error HTTP en TipoUsuarioService.GetAll: " +
                    $"{ex.Message}");

                return new List<TipoUsuarioDto>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Error JSON en TipoUsuarioService.GetAll: " +
                    $"{ex.Message}");

                return new List<TipoUsuarioDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error en TipoUsuarioService.GetAll: " +
                    $"{ex.Message}");

                return new List<TipoUsuarioDto>();
            }
        }
    }

    public class TipoUsuarioDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}