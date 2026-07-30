using SRV12_EstadoUsuario.Auth;
using SRV12_EstadoUsuario.Entities;
using SRV12_EstadoUsuario.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SRV12_EstadoUsuario
{
    public static class EstadoUsuarioEndpoints
    {
        public static void MapEstadoUsuarioEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/usuarios/estado")
                .WithTags("EstadoUsuario")
                .RequireCors("ReactDev");

            // PATCH / - Cambiar estado de un usuario (SRV12) - ACTUALIZACION
            group.MapMethods("/", new[] { "PATCH" }, async (
                HttpContext context,
                [FromServices] IEstadoUsuarioService service,
                [FromServices] IBitacoraClient bitacora,
                [FromBody] CambioEstadoRequest request) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (request is null || !request.UsuarioId.HasValue || request.UsuarioId <= 0 ||
                    string.IsNullOrWhiteSpace(request.Estado))
                    return Results.BadRequest(new { message = "El identificador del usuario y el codigo de estado son requeridos" });

                try
                {
                    var (result, usuario) = await service.CambiarEstadoAsync(request.UsuarioId.Value, request.Estado);

                    if (result == -1)
                        return Results.NotFound(new { message = $"El usuario con identificador {request.UsuarioId} no existe" });
                    if (result == -2)
                        return Results.NotFound(new { message = $"El estado '{request.Estado}' no existe" });
                    if (result <= 0)
                        return Results.Problem("No se pudo cambiar el estado");

                    // --- BITÁCORA: ACTUALIZACION ---
                    var usuarioBitacora = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "ACTUALIZACION",
                        EstadoUsuario = usuario
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuarioBitacora,
                        $"Actualizó el estado del usuario {request.UsuarioId} a '{request.Estado}'",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("El estado se actualizó, pero no se pudo registrar la bitácora.");
                    // -------------------------------

                    return Results.Ok(usuario);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token,
                        $"Error al cambiar el estado del usuario {request.UsuarioId}", ex);

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al cambiar el estado del usuario",
                        detail: ex.Message);
                }
            })
            .WithName("CambiarEstadoUsuario");
        }

        // ==========================================
        // REGISTRAR ERROR TÉCNICO EN BITÁCORA
        // ==========================================
        private static async Task RegistrarErrorBitacora(
            IBitacoraClient bitacora,
            string token,
            string accion,
            Exception ex)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            try
            {
                var usuario = ObtenerUsuarioDesdeToken(token);
                var detalleJson = JsonSerializer.Serialize(new
                {
                    Accion = "ERROR",
                    Mensaje = ex.Message,
                    Tipo = ex.GetType().Name,
                    Detalle = ex.StackTrace
                });

                await bitacora.RegistrarAsync(token, usuario, accion, detalleJson, true);
            }
            catch (Exception errorBitacora)
            {
                Console.WriteLine($"Error adicional al registrar la bitácora: {errorBitacora.Message}");
            }
        }

        // ==========================================
        // OBTENER TOKEN DEL HEADER
        // ==========================================
        private static string ObtenerToken(HttpContext context)
        {
            var header = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(header))
                return string.Empty;

            const string prefijo = "Bearer ";

            if (!header.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return header[prefijo.Length..].Trim();
        }

        // ==========================================
        // OBTENER USUARIO DESDE EL JWT
        // ==========================================
        private static string ObtenerUsuarioDesdeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return "Usuario desconocido";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var usuario =
                    BuscarClaim(jwt, "nombreCompleto") ??
                    BuscarClaim(jwt, "NombreCompleto") ??
                    BuscarClaim(jwt, "nombre") ??
                    BuscarClaim(jwt, "name") ??
                    BuscarClaim(jwt, "unique_name") ??
                    BuscarClaim(jwt, ClaimTypes.Name) ??
                    BuscarClaim(jwt, "email") ??
                    BuscarClaim(jwt, ClaimTypes.Email) ??
                    BuscarClaim(jwt, "sub") ??
                    BuscarClaim(jwt, ClaimTypes.NameIdentifier);

                return string.IsNullOrWhiteSpace(usuario) ? "Usuario desconocido" : usuario;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo leer el usuario del token: {ex.Message}");
                return "Usuario desconocido";
            }
        }

        private static string? BuscarClaim(JwtSecurityToken token, string tipo)
        {
            return token.Claims
                .FirstOrDefault(claim =>
                    string.Equals(claim.Type, tipo, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }
    }
}
