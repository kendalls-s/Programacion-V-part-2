using Microsoft.AspNetCore.Mvc;
using SRV15_Parametro.Auth;
using SRV15_Parametro.Entities;
using SRV15_Parametro.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SRV15_Parametro
{
    public static class ParametroEndpoints
    {
        public static void MapParametroEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/parametro")
                .WithTags("Parametro")
                .RequireCors("ReactDev");

            // GET /parametro - Obtener todos (CONSULTA)
            group.MapGet("/", async (
                HttpContext context,
                [FromServices] IParametroService service,
                [FromServices] IBitacoraClient bitacora) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                try
                {
                    var lista = await service.GetAllAsync();

                    // --- BITÁCORA: CONSULTA ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "CONSULTA",
                        Descripcion = "El usuario consultó los parámetros"
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        "Consultó los parámetros",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("La consulta se realizó, pero no se pudo registrar la bitácora.");
                    // --------------------------

                    return Results.Ok(lista);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token, "Error al consultar los parámetros", ex);
                    return Results.Problem(statusCode: 500, title: "Error al consultar los parámetros", detail: ex.Message);
                }
            })
            .WithName("GetAllParametros");

            // GET /parametro/{id} - Obtener por ID (CONSULTA)
            group.MapGet("/{id}", async (
                HttpContext context,
                string id,
                [FromServices] IParametroService service,
                [FromServices] IBitacoraClient bitacora) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(id))
                    return Results.BadRequest(new { message = "El identificador es requerido" });

                try
                {
                    var parametro = await service.GetByIdAsync(id);

                    if (parametro is null)
                        return Results.NotFound(new { message = $"No se encontró el parámetro '{id}'" });

                    // --- BITÁCORA: CONSULTA ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "CONSULTA",
                        Descripcion = $"El usuario consultó el parámetro {id}"
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Consultó el parámetro {id}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("La consulta se realizó, pero no se pudo registrar la bitácora.");
                    // --------------------------

                    return Results.Ok(parametro);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token, $"Error al consultar el parámetro {id}", ex);
                    return Results.Problem(statusCode: 500, title: "Error al consultar el parámetro", detail: ex.Message);
                }
            })
            .WithName("GetParametroById");

            // POST /parametro - Crear (CREACION)
            group.MapPost("/", async (
                HttpContext context,
                [FromBody] ParametroRequest request,
                [FromServices] IParametroService service,
                [FromServices] IBitacoraClient bitacora) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                try
                {
                    var (ok, error) = await service.CreateAsync(request);

                    if (!ok && error.Contains("Ya existe"))
                        return Results.Conflict(new { message = error });

                    if (!ok)
                        return Results.BadRequest(new { message = error });

                    var creado = await service.GetByIdAsync(request.Id);

                    // --- BITÁCORA: CREACION ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "CREACION",
                        Parametro = creado
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Creó el parámetro {request.Id}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("El parámetro se creó, pero no se pudo registrar la bitácora.");
                    // --------------------------

                    return Results.Created($"/api/parametro/{request.Id}", creado);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token, $"Error al crear el parámetro {request.Id}", ex);
                    return Results.Problem(statusCode: 500, title: "Error al crear el parámetro", detail: ex.Message);
                }
            })
            .WithName("CreateParametro");

            // PUT /parametro/{id} - Modificar (ACTUALIZACION)
            group.MapPut("/{id}", async (
                HttpContext context,
                string id,
                [FromBody] ParametroRequest request,
                [FromServices] IParametroService service,
                [FromServices] IBitacoraClient bitacora) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(id))
                    return Results.BadRequest(new { message = "El identificador es requerido" });

                try
                {
                    // Estado anterior para el detalle de la bitácora
                    var anterior = await service.GetByIdAsync(id);

                    var (ok, error) = await service.UpdateAsync(id, request);
                    if (!ok && error.Contains("No se encontró"))
                        return Results.NotFound(new { message = error });

                    if (!ok)
                        return Results.BadRequest(new { message = error });

                    var actualizado = await service.GetByIdAsync(id);

                    // --- BITÁCORA: ACTUALIZACION ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "ACTUALIZACION",
                        Anterior = anterior,
                        Nuevo = actualizado
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Modificó el parámetro {id}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("El parámetro se modificó, pero no se pudo registrar la bitácora.");
                    // -------------------------------

                    return Results.Ok(actualizado);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token, $"Error al modificar el parámetro {id}", ex);
                    return Results.Problem(statusCode: 500, title: "Error al modificar el parámetro", detail: ex.Message);
                }
            })
            .WithName("UpdateParametro");

            // DELETE /parametro/{id} - Eliminar (ELIMINACION)
            group.MapDelete("/{id}", async (
                HttpContext context,
                string id,
                [FromServices] IParametroService service,
                [FromServices] IBitacoraClient bitacora) =>
            {
                var token = ObtenerToken(context);
                var tokenValidator = context.RequestServices.GetRequiredService<ITokenValidator>();
                if (!await tokenValidator.ValidateAsync(token))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(id))
                    return Results.BadRequest(new { message = "El identificador es requerido" });

                try
                {
                    var eliminado = await service.GetByIdAsync(id);

                    var (ok, error) = await service.DeleteAsync(id);
                    if (!ok)
                        return Results.NotFound(new { message = error });

                    // --- BITÁCORA: ELIMINACION ---
                    var usuario = ObtenerUsuarioDesdeToken(token);
                    var detalleJson = JsonSerializer.Serialize(new
                    {
                        Accion = "ELIMINACION",
                        Eliminado = eliminado
                    });

                    var registroBitacora = await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Eliminó el parámetro {id}",
                        detalleJson);

                    if (!registroBitacora)
                        Console.WriteLine("El parámetro se eliminó, pero no se pudo registrar la bitácora.");
                    // ------------------------------

                    return Results.Ok(eliminado);
                }
                catch (Exception ex)
                {
                    await RegistrarErrorBitacora(bitacora, token, $"Error al eliminar el parámetro {id}", ex);
                    return Results.Problem(statusCode: 500, title: "Error al eliminar el parámetro", detail: ex.Message);
                }
            })
            .WithName("DeleteParametro");
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
