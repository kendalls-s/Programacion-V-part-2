namespace SRV2_Instituciones.Auth
{
    public class TokenValidator : ITokenValidator
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TokenValidator(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> ValidateAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine(
                    "No se recibió un token para validar.");

                return false;
            }

            string? baseUrl =
                _configuration["Services:LoginSRV1"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Console.WriteLine(
                    "No existe la configuración Services:LoginSRV1.");

                return false;
            }

            try
            {
                string url =
                    $"{baseUrl.TrimEnd('/')}" +
                    "/api/auth/validate" +
                    $"?token={Uri.EscapeDataString(token.Trim())}";

                Console.WriteLine(
                    $"Validando token en: {url}");

                HttpResponseMessage response =
                    await _httpClient.GetAsync(url);

                string contenido =
                    await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"VALIDATE STATUS: {(int)response.StatusCode} " +
                    $"{response.StatusCode}");

                Console.WriteLine(
                    $"VALIDATE RESPONSE: {contenido}");

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"No se pudo conectar con LoginSRV1: {ex.Message}");

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error al validar el token: {ex.Message}");

                return false;
            }
        }
    }
}