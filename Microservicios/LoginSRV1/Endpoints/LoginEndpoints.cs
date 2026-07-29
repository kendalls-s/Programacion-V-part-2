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

        private static async Task<IResult> LoginAsync(
            [FromBody] LoginRequestDto request,
            IAuthService authService,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== LoginAsync ===");
            logger.LogInformation($"Email: {request.Email}");

            if (string.IsNullOrEmpty(request.Email))
            {
                return Results.BadRequest(new { message = "El email es requerido" });
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return Results.BadRequest(new { message = "La contraseña es requerida" });
            }

            var result = await authService.LoginAsync(request);

            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Message ?? "Credenciales inválidas" });
            }

            return Results.Ok(new
            {
                success = result.Success,
                message = result.Message,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                tokenType = result.TokenType ?? "Bearer",
                expiresIn = result.ExpiresIn ?? 3600,
                user = result.User != null ? new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    nombreCompleto = result.User.NombreCompleto,
                    tipoUsuario = result.User.TipoUsuario,
                    activo = result.User.Activo,
                    tipoUsuarioId = result.User.TipoUsuarioId,
                    rolId = result.User.RolId
                } : null
            });
        }

        private static async Task<IResult> RefreshTokenAsync(
            [FromBody] RefreshTokenRequestDto request,
            IAuthService authService)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Results.BadRequest(new { message = "Refresh token es requerido" });
            }

            var result = await authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.Success)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                success = result.Success,
                message = result.Message,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                tokenType = result.TokenType ?? "Bearer",
                expiresIn = result.ExpiresIn ?? 3600,
                user = result.User != null ? new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    nombreCompleto = result.User.NombreCompleto,
                    tipoUsuario = result.User.TipoUsuario,
                    activo = result.User.Activo,
                    tipoUsuarioId = result.User.TipoUsuarioId,
                    rolId = result.User.RolId
                } : null
            });
        }

        // ✅ Logout SIN LogoutRequestDto - usa Header en su lugar
        private static async Task<IResult> LogoutAsync(
            [FromHeader(Name = "refresh_token")] string? refreshToken,
            IAuthService authService)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.BadRequest(new { message = "Refresh token es requerido" });
            }

            var result = await authService.LogoutAsync(refreshToken);
            return Results.Ok(new { success = result });
        }

        private static async Task<IResult> ValidateTokenAsync(
            [FromHeader(Name = "Authorization")] string? authorization,
            IAuthService authService)
        {
            var token = authorization?.Replace("Bearer ", "") ?? "";

            if (string.IsNullOrEmpty(token))
            {
                return Results.Unauthorized();
            }

            var isValid = await authService.ValidateTokenAsync(token);
            return isValid ? Results.Ok(new { valid = true }) : Results.Unauthorized();
        }
    }
}