using SRV13_Fotografia.Auth;
using SRV13_Fotografia.Entities;
using SRV13_Fotografia.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SRV13_Fotografia
{
    public static class FotografiaEndpoints
    {
        public static void MapFotografiaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/usuario/fotografia")
                .WithTags("Fotografia")
                .RequireCors("ReactDev");

            // PUT / - Actualizar (agregar o reemplazar) fotografía del usuario
            group.MapPut("/", async (
                HttpContext context,
                [FromServices] IFotografiaService service,
                [FromServices] IBitacoraClient bitacora,
                [FromBody] ActualizarFotografiaRequest request) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (request is null || !request.UsuarioId.HasValue || request.UsuarioId <= 0 ||
                    string.IsNullOrWhiteSpace(request.FotografiaBase64))
                    return Results.BadRequest(new { message = "El identificador del usuario y la fotografia son requeridos" });

                try
                {
                    var (result, mensaje, fotografia) = await service.ActualizarFotografiaAsync(
                        request.UsuarioId.Value, request.FotografiaBase64);

                    if (result == -1)
                        return Results.NotFound(new { message = $"El usuario con identificador {request.UsuarioId} no existe" });
                    if (result == -2)
                        return Results.BadRequest(new { message = mensaje });
                    if (result <= 0)
                        return Results.Problem("No se pudo actualizar la fotografia");

                    // --- BITÁCORA: ACTUALIZACION (no se guarda el Base64 completo) ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "ACTUALIZACION",
                        UsuarioId = request.UsuarioId
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Actualizó la fotografía del usuario {request.UsuarioId}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("La fotografía se actualizó, pero no se pudo registrar la bitácora.");
                    // ---------------------------------------------------------------

                    return Results.Ok(fotografia);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token,
                        $"Error al actualizar la fotografía del usuario {request.UsuarioId}", ex);

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al actualizar la fotografía",
                        detail: ex.Message);
                }
            })
            .WithName("ActualizarFotografia");

            // DELETE /{usuarioId} - Eliminar fotografía del usuario
            group.MapDelete("/{usuarioId:int}", async (
                HttpContext context,
                [FromServices] IFotografiaService service,
                [FromServices] IBitacoraClient bitacora,
                int usuarioId) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (usuarioId <= 0)
                    return Results.BadRequest(new { message = "El identificador del usuario es requerido" });

                try
                {
                    var (result, fotografia) = await service.EliminarFotografiaAsync(usuarioId);

                    if (result == -1)
                        return Results.NotFound(new { message = $"El usuario con identificador {usuarioId} no existe" });
                    if (result <= 0)
                        return Results.NotFound(new { message = $"El usuario con identificador {usuarioId} no tiene fotografia registrada" });

                    // --- BITÁCORA: ELIMINACION ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "ELIMINACION",
                        UsuarioId = usuarioId
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Eliminó la fotografía del usuario {usuarioId}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("La fotografía se eliminó, pero no se pudo registrar la bitácora.");
                    // ------------------------------

                    return Results.Ok(fotografia);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token,
                        $"Error al eliminar la fotografía del usuario {usuarioId}", ex);

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al eliminar la fotografía",
                        detail: ex.Message);
                }
            })
            .WithName("EliminarFotografia");

            // GET /{usuarioId} - Obtener fotografía del usuario en Base 64 (CONSULTA)
            group.MapGet("/{usuarioId:int}", async (
                HttpContext context,
                [FromServices] IFotografiaService service,
                [FromServices] IBitacoraClient bitacora,
                int usuarioId) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (usuarioId <= 0)
                    return Results.BadRequest(new { message = "El identificador del usuario es requerido" });

                try
                {
                    var foto = await service.ObtenerFotografiaAsync(usuarioId);

                    if (foto is null)
                        return Results.NotFound(new { message = $"No se encontro fotografia para el usuario con identificador {usuarioId}" });

                    // --- BITÁCORA: CONSULTA ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "CONSULTA",
                        Descripcion = $"El usuario consultó la fotografía del usuario {usuarioId}"
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Consultó la fotografía del usuario {usuarioId}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("La fotografía se consultó, pero no se pudo registrar la bitácora.");
                    // --------------------------

                    return Results.Ok(foto);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token,
                        $"Error al consultar la fotografía del usuario {usuarioId}", ex);

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al consultar la fotografía",
                        detail: ex.Message);
                }
            })
            .WithName("ObtenerFotografia");
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
