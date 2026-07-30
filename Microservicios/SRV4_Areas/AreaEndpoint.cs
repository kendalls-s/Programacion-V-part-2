using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using SRV4_Areas.Auth;
using SRV4_Areas.Entities;
using SRV4_Areas.Services;

namespace SRV4_Areas;

public static class AreaEndpoint
{
    public static void MapAreaEndpoints(
        this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes
            .MapGroup("/api/Area")
            .WithTags("Area");

        // ==========================================
        // GET TODOS - PÚBLICO
        // ==========================================
        group.MapGet("/", async (
            IAreaService service) =>
        {
            try
            {
                IEnumerable<AreaTrabajo> areas =
                    await service.GetAll();

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = areas
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar las áreas",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // GET POR ID - PÚBLICO
        // ==========================================
        group.MapGet("/{id:int}", async (
            int id,
            IAreaService service) =>
        {
            try
            {
                AreaTrabajo? area =
                    await service.GetById(id);

                if (area == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Área no encontrada"
                    });
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = area
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar el área",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // POST
        // ==========================================
        group.MapPost("/", async (
            CreateAreaRequest request,
            IAreaService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token =
                ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                (
                    bool success,
                    string message,
                    int? id
                ) resultado =
                    await service.Create(
                        request,
                        token
                    );

                if (!resultado.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = resultado.message
                    });
                }

                if (!resultado.id.HasValue)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje =
                            "No se pudo obtener el ID del área creada"
                    });
                }

                AreaTrabajo? creada =
                    await service.GetById(
                        resultado.id.Value
                    );

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "CREACION",
                        Area = creada
                    });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Creó el área {request.Nombre}",
                    detalleJson
                );

                return Results.Created(
                    $"/api/Area/{resultado.id.Value}",
                    new
                    {
                        codigo = 201,
                        mensaje =
                            "Área creada correctamente",
                        data = creada
                    }
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al crear el área",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // PUT
        // ==========================================
        group.MapPut("/{id:int}", async (
            int id,
            UpdateAreaRequest request,
            IAreaService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token =
                ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                AreaTrabajo? anterior =
                    await service.GetById(id);

                if (anterior == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Área no encontrada"
                    });
                }

                if (id != request.ID)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje =
                            "El ID de la ruta no coincide con el ID del cuerpo"
                    });
                }

                (
                    bool success,
                    string message
                ) resultado =
                    await service.Update(
                        request,
                        token
                    );

                if (!resultado.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = resultado.message
                    });
                }

                AreaTrabajo? actualizada =
                    await service.GetById(id);

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "ACTUALIZACION",
                        Anterior = anterior,
                        Nuevo = actualizada
                    });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Modificó el área {request.Nombre}",
                    detalleJson
                );

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje =
                        "Área actualizada correctamente",
                    data = actualizada
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al actualizar el área",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // DELETE
        // ==========================================
        group.MapDelete("/{id:int}", async (
            int id,
            IAreaService service,
            IBitacoraClient bitacora,
            ITokenValidator validator,
            HttpContext context) =>
        {
            string token =
                ObtenerToken(context);

            if (!await validator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                AreaTrabajo? area =
                    await service.GetById(id);

                if (area == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Área no encontrada"
                    });
                }

                (
                    bool success,
                    string message
                ) resultado =
                    await service.Delete(id);

                if (!resultado.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = resultado.message
                    });
                }

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "ELIMINACION",
                        Eliminado = area
                    });

                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    $"Eliminó el área {area.Nombre}",
                    detalleJson
                );

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje =
                        "Área eliminada correctamente"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    statusCode: 500,
                    title: "Error al eliminar el área",
                    detail: ex.Message
                );
            }
        });
    }

    // ==========================================
    // OBTENER TOKEN DEL HEADER
    // ==========================================
    private static string ObtenerToken(
        HttpContext context)
    {
        string header =
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
    // OBTENER USUARIO DESDE EL TOKEN
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
            JwtSecurityTokenHandler handler =
                new JwtSecurityTokenHandler();

            JwtSecurityToken jwt =
                handler.ReadJwtToken(token);

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
                BuscarClaim(
                    jwt,
                    ClaimTypes.NameIdentifier
                );

            return string.IsNullOrWhiteSpace(usuario)
                ? "Usuario desconocido"
                : usuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"No se pudo leer el usuario del token: {ex.Message}"
            );

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
                    StringComparison.OrdinalIgnoreCase
                )
            )
            ?.Value;
    }
}