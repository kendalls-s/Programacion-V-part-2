using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using TipoIdentificacionSRV6.Auth;
using TipoIdentificacionSRV6.DTOs;
using TipoIdentificacionSRV6.Services;

namespace TipoIdentificacionSRV6.Endpoints;

public static class TipoIdentificacionEndpoints
{
    public static void MapTipoIdentificacionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/TipoIdentificacion")
            .WithTags("TipoIdentificacion");

        // ==========================================
        // ✅ GET TODOS - SIN TOKEN
        // ==========================================
        group.MapGet("/", async (ITipoIdentificacionService service) =>
        {
            var tipos = await service.ObtenerTodosAsync();

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "OK",
                data = tipos
            });
        });

        // ==========================================
        // ✅ GET POR ID - SIN TOKEN
        // ==========================================
        group.MapGet("/{id:int}", async (int id, ITipoIdentificacionService service) =>
        {
            var tipo = await service.ObtenerPorIdAsync(id);

            if (tipo == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Tipo de identificación no encontrado"
                });
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "OK",
                data = tipo
            });
        });

        // ==========================================
        // ✅ POST - CON TOKEN Y BITÁCORA
        // ==========================================
        group.MapPost("/", async (
            TipoIdentificacionCreateDto request,
            ITipoIdentificacionService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var resultado = await service.CrearAsync(request);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var creado = await service.ObtenerPorIdAsync(resultado.id);

            var usuario = ObtenerUsuarioDesdeToken(token);

            var detalleJson = JsonSerializer.Serialize(new
            {
                Accion = "CREACION",
                TipoIdentificacion = creado
            });

            var registroBitacora = await bitacora.RegistrarAsync(
                token,
                usuario,
                $"Creó el tipo de identificación {request.Nombre}",
                detalleJson
            );

            if (!registroBitacora)
            {
                Console.WriteLine("El tipo de identificación se creó, pero no se pudo registrar la bitácora.");
            }

            return Results.Created(
                $"/api/TipoIdentificacion/{resultado.id}",
                new
                {
                    codigo = 201,
                    mensaje = "Tipo de identificación creado correctamente",
                    data = creado
                });
        });

        // ==========================================
        // ✅ PUT - CON TOKEN Y BITÁCORA
        // ==========================================
        group.MapPut("/{id:int}", async (
            int id,
            TipoIdentificacionUpdateDto request,
            ITipoIdentificacionService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var anterior = await service.ObtenerPorIdAsync(id);

            if (anterior == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Tipo de identificación no encontrado"
                });
            }

            var resultado = await service.ActualizarAsync(id, request);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var actualizado = await service.ObtenerPorIdAsync(id);

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
                $"Modificó el tipo de identificación {request.Nombre}",
                detalleJson
            );

            if (!registroBitacora)
            {
                Console.WriteLine("El tipo de identificación se modificó, pero no se pudo registrar la bitácora.");
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "Tipo de identificación actualizado correctamente",
                data = actualizado
            });
        });

        // ==========================================
        // ✅ DELETE - CON TOKEN Y BITÁCORA
        // ==========================================
        group.MapDelete("/{id:int}", async (
            int id,
            ITipoIdentificacionService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            var token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            var tipo = await service.ObtenerPorIdAsync(id);

            if (tipo == null)
            {
                return Results.NotFound(new
                {
                    codigo = 404,
                    mensaje = "Tipo de identificación no encontrado"
                });
            }

            var resultado = await service.EliminarAsync(id);

            if (!resultado.ok)
            {
                return Results.BadRequest(new
                {
                    codigo = 400,
                    mensaje = resultado.error
                });
            }

            var usuario = ObtenerUsuarioDesdeToken(token);

            var detalleJson = JsonSerializer.Serialize(new
            {
                Accion = "ELIMINACION",
                Eliminado = tipo
            });

            var registroBitacora = await bitacora.RegistrarAsync(
                token,
                usuario,
                $"Eliminó el tipo de identificación {tipo.Nombre}",
                detalleJson
            );

            if (!registroBitacora)
            {
                Console.WriteLine("El tipo de identificación se eliminó, pero no se pudo registrar la bitácora.");
            }

            return Results.Ok(new
            {
                codigo = 200,
                mensaje = "Tipo de identificación eliminado correctamente"
            });
        });

        // ==========================================
        // ✅ EXISTS - SIN TOKEN
        // ==========================================
        group.MapGet("/exists/{id:int}", async (int id, ITipoIdentificacionService service) =>
        {
            var exists = await service.ExisteAsync(id);
            return Results.Ok(new { exists });
        });

        // ==========================================
        // ✅ EXISTS POR NOMBRE - SIN TOKEN
        // ==========================================
        group.MapGet("/exists/nombre/{nombre}", async (string nombre, int? excludeId, ITipoIdentificacionService service) =>
        {
            var exists = await service.ExisteNombreAsync(nombre, excludeId);
            return Results.Ok(new { exists });
        });
    }

    // ==========================================
    // ✅ OBTENER TOKEN DEL HEADER
    // ==========================================
    private static string ObtenerToken(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(header))
        {
            return string.Empty;
        }

        const string prefijo = "Bearer ";

        if (!header.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return header[prefijo.Length..].Trim();
    }

    // ==========================================
    // ✅ OBTENER USUARIO DESDE EL JWT
    // ==========================================
    private static string ObtenerUsuarioDesdeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "Usuario desconocido";
        }

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

            return string.IsNullOrWhiteSpace(usuario)
                ? "Usuario desconocido"
                : usuario;
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