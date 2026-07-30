using SRV11_AutoRegistro.Entities;
using SRV11_AutoRegistro.Repository;
using SRV11_AutoRegistro.Services;

namespace SRV11_AutoRegistro;

public static class AutoRegistroEndpoints
{
    public static void MapAutoRegistroEndpoints(
        this WebApplication app)
    {
        var group = app
            .MapGroup("/autoregistro")
            .WithTags("AutoRegistro");

        group.MapGet(
            "/roles",
            async (RolRepository rolRepository) =>
            {
                try
                {
                    var roles =
                        await rolRepository.ObtenerTodosAsync();

                    return Results.Ok(new
                    {
                        codigo = 200,
                        mensaje =
                            "Roles obtenidos correctamente",
                        data = roles
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error obteniendo roles: {ex.Message}");

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error interno",
                        detail:
                            "No se pudieron obtener los roles.");
                }
            });

        group.MapPost(
            "/",
            async (
                Usuario usuario,
                IUsuarioService service) =>
            {
                try
                {
                    var resultado =
                        await service.RegistrarAsync(usuario);

                    if (!resultado.ok)
                    {
                        return Results.BadRequest(new
                        {
                            codigo = 400,
                            mensaje = resultado.error
                        });
                    }

                    if (resultado.usuarioCreado is null)
                    {
                        return Results.Problem(
                            statusCode: 500,
                            title: "Error interno",
                            detail:
                                "No fue posible obtener el usuario creado.");
                    }

                    var usuarioCreado =
                        resultado.usuarioCreado;

                    return Results.Ok(new
                    {
                        codigo = 200,
                        mensaje =
                            "Usuario registrado correctamente. Revise su correo para confirmar la cuenta.",
                        data = new
                        {
                            id = usuarioCreado.Id,
                            email = usuarioCreado.Email,
                            nombreCompleto =
                                usuarioCreado.NombreCompleto,
                            numeroIdentificacion =
                                usuarioCreado
                                    .NumeroIdentificacion,
                            tipoUsuarioId =
                                usuarioCreado.TipoUsuarioId,
                            tipoIdentificacionId =
                                usuarioCreado
                                    .TipoIdentificacionId,
                            rolId = usuarioCreado.RolId,
                            instituciones =
                                usuarioCreado.Instituciones,
                            carrerasAsociadas =
                                usuarioCreado
                                    .CarrerasAsociadas,
                            areasAsociadas =
                                usuarioCreado.AreasAsociadas,
                            telefonos =
                                usuarioCreado.Telefonos,
                            estadoId =
                                usuarioCreado.EstadoId,
                            confirmado =
                                usuarioCreado.Confirmado,
                            fechaCreacion =
                                usuarioCreado.FechaCreacion
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error registrando usuario: {ex.Message}");

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error interno",
                        detail:
                            "Ocurrió un error al registrar el usuario.");
                }
            });

        group.MapGet(
            "/confirmar/{token}",
            async (
                string token,
                IUsuarioService service) =>
            {
                try
                {
                    var resultado =
                        await service.ConfirmarCuentaAsync(
                            token);

                    if (!resultado.ok)
                    {
                        return Results.BadRequest(new
                        {
                            codigo = 400,
                            mensaje = resultado.error
                        });
                    }

                    return Results.Ok(new
                    {
                        codigo = 200,
                        mensaje =
                            "Cuenta confirmada correctamente"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error confirmando cuenta: {ex.Message}");

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error interno",
                        detail:
                            "Ocurrió un error al confirmar la cuenta.");
                }
            });
    }
}