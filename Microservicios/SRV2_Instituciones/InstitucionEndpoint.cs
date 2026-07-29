using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using SRV2_Instituciones.Auth;
using SRV2_Instituciones.Entities;
using SRV2_Instituciones.Services;

namespace SRV2_Instituciones;

public static class InstitucionEndpoint
{
    public static void MapInstitucionEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/Institucion")
            .WithTags("Institucion");

        // ==========================================
        // GET TODOS - PÚBLICO
        // ==========================================
        group.MapGet("/", async (
            IInstitucionService service,
            IBitacoraClient bitacora,
            HttpContext context) =>
        {
            try
            {
                IEnumerable<Institucion> instituciones =
                    await service.GetAll();

                /*
                 * Este endpoint es público porque será utilizado
                 * desde el autoregistro, antes de que el usuario
                 * haya iniciado sesión.
                 *
                 * Si viene un token válido, se puede registrar
                 * la consulta en bitácora. Si no viene token,
                 * la consulta igualmente se permite.
                 */
                string token = ObtenerToken(context);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    string usuario =
                        ObtenerUsuarioDesdeToken(token);

                    string detalleJson =
                        JsonSerializer.Serialize(new
                        {
                            Accion = "CONSULTA",
                            Descripcion =
                                "El usuario consulta las instituciones"
                        });

                    bool registroBitacora =
                        await bitacora.RegistrarAsync(
                            token,
                            usuario,
                            "El usuario consulta las instituciones",
                            detalleJson
                        );

                    if (!registroBitacora)
                    {
                        Console.WriteLine(
                            "La consulta se realizó, pero no se pudo registrar la bitácora.");
                    }
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = instituciones
                });
            }
            catch (Exception ex)
            {
                /*
                 * Como este endpoint puede consumirse sin token,
                 * solamente registramos el error en bitácora
                 * cuando exista un token.
                 */
                string token = ObtenerToken(context);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await RegistrarErrorBitacora(
                        bitacora,
                        token,
                        "Error al consultar las instituciones",
                        ex
                    );
                }

                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar las instituciones",
                    detail: ex.Message
                );
            }
        });
        // ==========================================
        // GET POR ID
        // ==========================================
        group.MapGet("/{id:int}", async (
            int id,
            IInstitucionService service,
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
                Institucion? institucion =
                    await service.GetById(id);

                if (institucion == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Institución no encontrada"
                    });
                }

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "CONSULTA",
                        Descripcion =
                            $"El usuario consulta la institución {id}"
                    });

                bool registroBitacora =
                    await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"El usuario consulta la institución {id}",
                        detalleJson
                    );

                if (!registroBitacora)
                {
                    Console.WriteLine(
                        "La consulta se realizó, pero no se pudo registrar la bitácora.");
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "OK",
                    data = institucion
                });
            }
            catch (Exception ex)
            {
                await RegistrarErrorBitacora(
                    bitacora,
                    token,
                    $"Error al consultar la institución {id}",
                    ex
                );

                return Results.Problem(
                    statusCode: 500,
                    title: "Error al consultar la institución",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // POST
        // ==========================================
        group.MapPost("/", async (
            CreateInstitucionRequest request,
            IInstitucionService service,
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
                (bool success, string message, int? id) resultado =
                    await service.Create(request);

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
                            "No se pudo obtener el ID de la institución creada"
                    });
                }

                Institucion? creado =
                    await service.GetById(resultado.id.Value);

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "CREACION",
                        Institucion = creado
                    });

                bool registroBitacora =
                    await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Creó la institución {request.Nombre}",
                        detalleJson
                    );

                if (!registroBitacora)
                {
                    Console.WriteLine(
                        "La institución se creó, pero no se pudo registrar la bitácora.");
                }

                return Results.Created(
                    $"/api/Institucion/{resultado.id.Value}",
                    new
                    {
                        codigo = 201,
                        mensaje = "Institución creada correctamente",
                        data = creado
                    });
            }
            catch (Exception ex)
            {
                await RegistrarErrorBitacora(
                    bitacora,
                    token,
                    $"Error al crear la institución {request.Nombre}",
                    ex
                );

                return Results.Problem(
                    statusCode: 500,
                    title: "Error al crear la institución",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // PUT
        // ==========================================
        group.MapPut("/{id:int}", async (
            int id,
            UpdateInstitucionRequest request,
            IInstitucionService service,
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
                Institucion? anterior =
                    await service.GetById(id);

                if (anterior == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Institución no encontrada"
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

                (bool success, string message) resultado =
                    await service.Update(request);

                if (!resultado.success)
                {
                    return Results.BadRequest(new
                    {
                        codigo = 400,
                        mensaje = resultado.message
                    });
                }

                Institucion? actualizado =
                    await service.GetById(id);

                string usuario =
                    ObtenerUsuarioDesdeToken(token);

                string detalleJson =
                    JsonSerializer.Serialize(new
                    {
                        Accion = "ACTUALIZACION",
                        Anterior = anterior,
                        Nuevo = actualizado
                    });

                bool registroBitacora =
                    await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Modificó la institución {request.Nombre}",
                        detalleJson
                    );

                if (!registroBitacora)
                {
                    Console.WriteLine(
                        "La institución se modificó, pero no se pudo registrar la bitácora.");
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "Institución actualizada correctamente",
                    data = actualizado
                });
            }
            catch (Exception ex)
            {
                await RegistrarErrorBitacora(
                    bitacora,
                    token,
                    $"Error al modificar la institución {id}",
                    ex
                );

                return Results.Problem(
                    statusCode: 500,
                    title: "Error al modificar la institución",
                    detail: ex.Message
                );
            }
        });

        // ==========================================
        // DELETE
        // ==========================================
        group.MapDelete("/{id:int}", async (
            int id,
            IInstitucionService service,
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
                Institucion? institucion =
                    await service.GetById(id);

                if (institucion == null)
                {
                    return Results.NotFound(new
                    {
                        codigo = 404,
                        mensaje = "Institución no encontrada"
                    });
                }

                (bool success, string message) resultado =
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
                        Eliminado = institucion
                    });

                bool registroBitacora =
                    await bitacora.RegistrarAsync(
                        token,
                        usuario,
                        $"Eliminó la institución {institucion.Nombre}",
                        detalleJson
                    );

                if (!registroBitacora)
                {
                    Console.WriteLine(
                        "La institución se eliminó, pero no se pudo registrar la bitácora.");
                }

                return Results.Ok(new
                {
                    codigo = 200,
                    mensaje = "Institución eliminada correctamente"
                });
            }
            catch (Exception ex)
            {
                await RegistrarErrorBitacora(
                    bitacora,
                    token,
                    $"Error al eliminar la institución {id}",
                    ex
                );

                return Results.Problem(
                    statusCode: 500,
                    title: "Error al eliminar la institución",
                    detail: ex.Message
                );
            }
        });
    }

    // ==========================================
    // REGISTRAR ERROR TÉCNICO
    // ==========================================
    private static async Task RegistrarErrorBitacora(
        IBitacoraClient bitacora,
        string token,
        string accion,
        Exception ex)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        try
        {
            string usuario =
                ObtenerUsuarioDesdeToken(token);

            string detalleJson =
                JsonSerializer.Serialize(new
                {
                    Accion = "ERROR",
                    Mensaje = ex.Message,
                    Tipo = ex.GetType().Name,
                    Detalle = ex.StackTrace
                });

            bool registrado =
                await bitacora.RegistrarAsync(
                    token,
                    usuario,
                    accion,
                    detalleJson,
                    true
                );

            if (!registrado)
            {
                Console.WriteLine(
                    "No se pudo registrar el error técnico en la bitácora.");
            }
        }
        catch (Exception errorBitacora)
        {
            Console.WriteLine(
                $"Error adicional al registrar la bitácora: " +
                $"{errorBitacora.Message}");
        }
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