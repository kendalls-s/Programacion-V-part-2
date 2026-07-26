using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using RolSRV8.Auth;
using RolSRV8.Entities;
using RolSRV8.Services;

namespace RolSRV8;

public static class RolEndpoints
{
    public static void MapRolEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/Rol")
            .WithTags("Rol");

        // ==========================================
        // GET TODOS
        // ==========================================
        group.MapGet("/", async (
            IRolService service,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var roles =
                await service.ObtenerTodosAsync();

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "OK",
                data = roles
            });
        });

        // ==========================================
        // GET POR ID
        // ==========================================
        group.MapGet("/{id:int}", async (
            int id,
            IRolService service,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var rol =
                await service.ObtenerPorIdAsync(id);

            if (rol == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Rol no encontrado"
                });
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "OK",
                data = rol
            });
        });

        // ==========================================
        // POST
        // ==========================================
        group.MapPost("/", async (
            RolRequest request,
            IRolService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var resultado =
                await service.CrearAsync(request);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var creado =
                await service.ObtenerPorIdAsync(resultado.id);

            var usuario =
                ObtenerUsuarioDesdeToken(token);

            var detalleJson =
                JsonSerializer.Serialize(new
                {
                    Accion = "CREACION",
                    Rol = creado
                });

            var registroBitacora =
                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Creó el rol {request.Nombre}",
                    detalleJson
                );

            if (!registroBitacora)
            {
                Console.WriteLine(
                    "El rol se creó, pero no se pudo registrar la bitácora.");
            }

            return Results.Created(
                $"/api/Rol/{resultado.id}",
                new
                {
                    codigo = 201,
                    mensaje = "Rol creado correctamente",
                    data = creado
                });
        });

        // ==========================================
        // PUT
        // ==========================================
        group.MapPut("/{id:int}", async (
            int id,
            RolRequest request,
            IRolService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var anterior =
                await service.ObtenerPorIdAsync(id);

            if (anterior == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Rol no encontrado"
                });
            }

            var resultado =
                await service.ActualizarAsync(
                    id,
                    request);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var actualizado =
                await service.ObtenerPorIdAsync(id);

            var usuario =
                ObtenerUsuarioDesdeToken(token);

            var detalleJson =
                JsonSerializer.Serialize(new
                {
                    Accion = "ACTUALIZACION",
                    Anterior = anterior,
                    Nuevo = actualizado
                });

            var registroBitacora =
                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Modificó el rol {request.Nombre}",
                    detalleJson
                );

            if (!registroBitacora)
            {
                Console.WriteLine(
                    "El rol se modificó, pero no se pudo registrar la bitácora.");
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "Rol actualizado correctamente",
                data = actualizado
            });
        });

        // ==========================================
        // DELETE
        // ==========================================
        group.MapDelete("/{id:int}", async (
            int id,
            IRolService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var rol =
                await service.ObtenerPorIdAsync(id);

            if (rol == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Rol no encontrado"
                });
            }

            var resultado =
                await service.EliminarAsync(id);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var usuario =
                ObtenerUsuarioDesdeToken(token);

            var detalleJson =
                JsonSerializer.Serialize(new
                {
                    Accion = "ELIMINACION",
                    Eliminado = rol
                });

            var registroBitacora =
                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Eliminó el rol {rol.Nombre}",
                    detalleJson
                );

            if (!registroBitacora)
            {
                Console.WriteLine(
                    "El rol se eliminó, pero no se pudo registrar la bitácora.");
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "Rol eliminado correctamente"
            });
        });
    }

    // ==========================================
    // OBTENER TOKEN DEL HEADER
    // ==========================================
    private static string ObtenerToken(
        HttpContext context)
    {
        var header =
            context.Request.Headers.Authorization
                .ToString();

        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        const string prefijo = "Bearer ";

        if (!header.StartsWith(
                prefijo,
                StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return header[prefijo.Length..].Trim();
    }

    // ==========================================
    // OBTENER USUARIO DESDE EL JWT
    // ==========================================
    private static string ObtenerUsuarioDesdeToken(
        string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "Usuario desconocido";
        }

        try
        {
            var handler =
                new JwtSecurityTokenHandler();

            var jwt =
                handler.ReadJwtToken(token);

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
                BuscarClaim(
                    jwt,
                    ClaimTypes.NameIdentifier);

            return string.IsNullOrWhiteSpace(usuario)
                ? "Usuario desconocido"
                : usuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"No se pudo leer el usuario del token: {ex.Message}");

            return "Usuario desconocido";
        }
    }

    private static string? BuscarClaim(
        JwtSecurityToken token,
        string tipo)
    {
        return token.Claims
            .FirstOrDefault(claim =>
                string.Equals(
                    claim.Type,
                    tipo,
                    StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }
}