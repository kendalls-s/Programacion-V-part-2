using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using SRV14_CarnetQR.Auth;
using SRV14_CarnetQR.Services;

namespace SRV14_CarnetQR
{
    public static class CarnetQREndpoints
    {
        public static void MapCarnetQREndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/usuario/qr")
                .WithTags("CarnetQR")
                .RequireCors("ReactDev");

            // GET /usuario/qr?identificacion={id} (SRV14) - Genera el QR y registra la CONSULTA
            group.MapGet("/", async (
                HttpContext context,
                [FromServices] ICarnetQRService service,
                [FromServices] IBitacoraClient bitacora,
                [FromQuery] string identificacion) =>
            {
                // --- AUTENTICACIÓN contra el validate del SRV1 ---
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(identificacion))
                    return Results.BadRequest(new { message = "La identificación del usuario es requerida" });

                try
                {
                    var qrBase64 = await service.GenerarQRAsync(identificacion);

                    if (qrBase64 is null)
                        return Results.NotFound(new { message = $"No se encontró el usuario con identificación '{identificacion}'" });

                    // --- BITÁCORA: se registra la consulta del carnet QR ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "CONSULTA",
                        Descripcion = $"El usuario consultó el carnet QR de la identificación {identificacion}"
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Consultó el carnet QR de la identificación {identificacion}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("El QR se generó, pero no se pudo registrar la bitácora.");
                    // ------------------------------------------------------

                    return Results.Ok(qrBase64);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(
                        bitacora,
                        token,
                        $"Error al generar el carnet QR de la identificación {identificacion}",
                        ex);

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al generar el carnet QR",
                        detail: ex.Message);
                }
            })
            .WithName("ObtenerCarnetQR")
            .WithSummary("Genera y devuelve el QR del carnet digital para un usuario");
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
