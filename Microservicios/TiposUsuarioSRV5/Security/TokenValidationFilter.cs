namespace TiposUsuarioSRV5.Security
{
    /// <summary>
    /// Endpoint filter que exige un token válido (emitido por LoginSRV1) en el header Authorization.
    /// El token se valida llamando al endpoint GET /api/auth/validate del servicio de login,
    /// enviando el token en el header "token", tal como lo espera SRV1.
    /// </summary>
    public class TokenValidationFilter : IEndpointFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenValidationFilter> _logger;

        public TokenValidationFilter(IHttpClientFactory httpClientFactory, ILogger<TokenValidationFilter> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var httpContext = context.HttpContext;

            var token = ExtractToken(httpContext);

            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Json(new { message = "No autorizado" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var client = _httpClientFactory.CreateClient("LoginApi");
            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/validate");
            request.Headers.Add("token", token);

            try
            {
                var response = await client.SendAsync(request, httpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(new { message = "No autorizado" }, statusCode: StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el token contra el servicio de login");
                return Results.Json(new { message = "No fue posible validar el token" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            return await next(context);
        }

        // El token se puede enviar como "Authorization: Bearer <token>" o directamente en un header "token".
        private static string? ExtractToken(HttpContext httpContext)
        {
            var authHeader = httpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : authHeader.Trim();
            }

            var tokenHeader = httpContext.Request.Headers["token"].ToString();
            return string.IsNullOrWhiteSpace(tokenHeader) ? null : tokenHeader.Trim();
        }
    }
}
