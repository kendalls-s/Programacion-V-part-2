using LoginSRV1.DTOs;
using LoginSRV1.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginSRV1.Endpoints
{
    public static class LoginEndpoints
    {
        public static void MapLoginEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/login", LoginAsync);
            group.MapPost("/refresh", RefreshTokenAsync);
            group.MapPost("/logout", LogoutAsync);
            group.MapGet("/validate", ValidateTokenAsync);
        }

        // POST /api/auth/login
        // Headers requeridos: usuario, password, tipo
        private static async Task<IResult> LoginAsync(
            HttpContext httpContext,
            IAuthService authService,
            [FromHeader(Name = "usuario")] string? usuario,
            [FromHeader(Name = "password")] string? password,
            [FromHeader(Name = "tipo")] string? tipo)
        {
            var result = await authService.LoginAsync(usuario, password, tipo);

            if (!result.Success)
            {
                return result.ErrorType == AuthErrorType.Validation
                    ? Results.BadRequest(new ErrorResponseDto { Message = result.ErrorMessage! })
                    : Results.Json(new ErrorResponseDto { Message = result.ErrorMessage! }, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Json(result.Data, statusCode: StatusCodes.Status201Created);
        }

        // POST /api/auth/refresh
        // Header requerido: refresh_token
        private static async Task<IResult> RefreshTokenAsync(
            [FromHeader(Name = "refresh_token")] string? refreshToken,
            IAuthService authService)
        {
            var result = await authService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
            {
                return Results.Json(new ErrorResponseDto { Message = result.ErrorMessage! }, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Json(result.Data, statusCode: StatusCodes.Status201Created);
        }

        private static async Task<IResult> LogoutAsync(
            [FromBody] RefreshTokenRequestDto request,
            IAuthService authService)
        {
            var result = await authService.LogoutAsync(request.RefreshToken);
            return Results.Ok(new { success = result });
        }

        // GET /api/auth/validate
        // Header requerido: token
        private static async Task<IResult> ValidateTokenAsync(
            [FromHeader(Name = "token")] string? token,
            IAuthService authService)
        {
            var isValid = await authService.ValidateTokenAsync(token);

            return isValid
                ? Results.Json(true, statusCode: StatusCodes.Status200OK)
                : Results.StatusCode(StatusCodes.Status401Unauthorized);
        }
    }
}
