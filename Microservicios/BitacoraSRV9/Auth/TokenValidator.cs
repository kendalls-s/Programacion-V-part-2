namespace BitacoraSRV9.Auth
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

            var baseUrl =
                _configuration["Services:LoginSRV1"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Console.WriteLine(
                    "No existe la configuración Services:LoginSRV1.");

                return false;
            }

            try
            {
                var url =
                    $"{baseUrl.TrimEnd('/')}" +
                    $"/api/auth/validate" +
                    $"?token={Uri.EscapeDataString(token.Trim())}";

                Console.WriteLine(
                    $"Validando token en: {url}");

                var response =
                    await _httpClient.GetAsync(url);

                var contenido =
                    await response.Content
                        .ReadAsStringAsync();

                Console.WriteLine(
                    $"VALIDATE STATUS: {response.StatusCode}");

                Console.WriteLine(
                    $"VALIDATE RESPONSE: {contenido}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Error al validar el token.");

                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}