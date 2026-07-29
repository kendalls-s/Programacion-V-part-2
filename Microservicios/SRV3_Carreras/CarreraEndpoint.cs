using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using SRV3_Carreras.Auth;
using SRV3_Carreras.Entities;
using SRV3_Carreras.Services;

namespace SRV3_Carreras;

public static class CarreraEndpoint
{
    public static void MapCarreraEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/Carrera")
            .WithTags("Carrera");

        // ==========================================
        // GET TODOS - PÚBLICO
        // ==========================================
        group.MapGet("/", async (ICarreraService service) =>
        {
            try
            {
                var carreras = await service.GetAll();
                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = carreras
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar las carreras",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // GET POR ID - REQUIERE AUTENTICACIÓN
        // ==========================================
        group.MapGet("/{id:int}", async (
            int id,
            ICarreraService service,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var carrera = await service.GetById(id);

                if (carrera == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Carrera no encontrada"
                    });
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = carrera
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar la carrera",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // POST - REQUIERE AUTENTICACIÓN
        // ==========================================
        group.MapPost("/", async (
            CreateCarreraRequest request,
            ICarreraService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                // ✅ PASAR EL TOKEN AL MÉTODO CREATE
                var result = await service.Create(request, token);

                if (!result.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = result.message
                    });
                }

                if (!result.id.HasValue)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = "No se pudo obtener el ID de la carrera creada"
                    });
                }

                var creada = await service.GetById(result.id.Value);

                string usuario = ObtenerUsuarioDesdeToken(token);

                string detalleJson = JsonSerializer.Serialize(new
                {
                    Accion = "CREACION",
                    Carrera = creada
                });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Creó la carrera {request.Nombre}",
                    detalleJson
                );

                return Results.Created($"/api/Carrera/{result.id.Value}", new
                {
                    codigo = 201,
                    mensaje = "Carrera creada correctamente",
                    data = creada
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al crear la carrera",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // PUT - REQUIERE AUTENTICACIÓN
        // ==========================================
        group.MapPut("/{id:int}", async (
            int id,
            UpdateCarreraRequest request,
            ICarreraService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var anterior = await service.GetById(id);

                if (anterior == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Carrera no encontrada"
                    });
                }

                if (id != request.ID)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = "El ID de la ruta no coincide con el ID del cuerpo"
                    });
                }

                // ✅ PASAR EL TOKEN AL MÉTODO UPDATE
                var result = await service.Update(request, token);

                if (!result.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = result.message
                    });
                }

                var actualizada = await service.GetById(id);

                string usuario = ObtenerUsuarioDesdeToken(token);

                string detalleJson = JsonSerializer.Serialize(new
                {
                    Accion = "ACTUALIZACION",
                    Anterior = anterior,
                    Nuevo = actualizada
                });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Modificó la carrera {request.Nombre}",
                    detalleJson
                );

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "Carrera actualizada correctamente",
                    data = actualizada
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al actualizar la carrera",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // DELETE - REQUIERE AUTENTICACIÓN
        // ==========================================
        group.MapDelete("/{id:int}", async (
            int id,
            ICarreraService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token = ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var carrera = await service.GetById(id);

                if (carrera == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Carrera no encontrada"
                    });
                }

                var result = await service.Delete(id);

                if (!result.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = result.message
                    });
                }

                string usuario = ObtenerUsuarioDesdeToken(token);

                string detalleJson = JsonSerializer.Serialize(new
                {
                    Accion = "ELIMINACION",
                    Eliminado = carrera
                });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Eliminó la carrera {carrera.Nombre}",
                    detalleJson
                );

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "Carrera eliminada correctamente"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al eliminar la carrera",
                    detail: ex.Message
                );
            }
        });
    }

    // ==========================================
    // OBTENER TOKEN DEL HEADER
    // ==========================================
    private static string ObtenerToken(HttpContext context)
    {
        string header = context.Request.Headers.Authorization.ToString();

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
    // OBTENER USUARIO DESDE EL TOKEN
    // ==========================================
    private static string ObtenerUsuarioDesdeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return "Usuario desconocido";
        }

        try
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            string? usuario =
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