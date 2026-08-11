using System.Net;
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
                                usuarioCreado.NumeroIdentificacion,
                            tipoUsuarioId =
                                usuarioCreado.TipoUsuarioId,
                            tipoIdentificacionId =
                                usuarioCreado.TipoIdentificacionId,
                            rolId = usuarioCreado.RolId,
                            instituciones =
                                usuarioCreado.Instituciones,
                            carrerasAsociadas =
                                usuarioCreado.CarrerasAsociadas,
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

        // Abre una página, pero todavía no confirma.
        group.MapGet(
    "/confirmar/{token}",
    (
        string token,
        HttpRequest request) =>
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.Content(
                CrearPaginaResultado(
                    "Enlace inválido",
                    "No se recibió un token de confirmación válido.",
                    false),
                "text/html",
                statusCode: 400);
        }

        var tokenSeguro =
            WebUtility.HtmlEncode(token.Trim());

        var pathBase =
            request.PathBase.Value ?? string.Empty;

        var accionFormulario =
            $"{pathBase}/autoregistro/confirmar";

        var html =
            CrearPaginaConfirmacion(
                tokenSeguro,
                WebUtility.HtmlEncode(accionFormulario));

        return Results.Content(
            html,
            "text/html");
    });

        // La confirmación real ocurre únicamente al presionar el botón.
        group.MapPost(
            "/confirmar",
            async (
                HttpRequest request,
                IUsuarioService service) =>
            {
                try
                {
                    if (!request.HasFormContentType)
                    {
                        return Results.Content(
                            CrearPaginaResultado(
                                "Solicitud inválida",
                                "No se recibió el formulario de confirmación.",
                                false),
                            "text/html",
                            statusCode: 400);
                    }

                    var formulario =
                        await request.ReadFormAsync();

                    var token =
                        formulario["token"]
                            .ToString()
                            .Trim();

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return Results.Content(
                            CrearPaginaResultado(
                                "Enlace inválido",
                                "No se recibió un token de confirmación válido.",
                                false),
                            "text/html",
                            statusCode: 400);
                    }

                    var resultado =
                        await service.ConfirmarCuentaAsync(
                            token);

                    if (!resultado.ok)
                    {
                        return Results.Content(
                            CrearPaginaResultado(
                                "No se pudo confirmar la cuenta",
                                resultado.error,
                                false),
                            "text/html",
                            statusCode: 400);
                    }

                    return Results.Content(
                        CrearPaginaResultado(
                            "Cuenta confirmada",
                            "Su cuenta fue confirmada correctamente. Ya puede iniciar sesión.",
                            true),
                        "text/html");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error confirmando cuenta: {ex.Message}");

                    return Results.Content(
                        CrearPaginaResultado(
                            "Error interno",
                            "Ocurrió un error al confirmar la cuenta.",
                            false),
                        "text/html",
                        statusCode: 500);
                }
            });
    }

    private static string CrearPaginaConfirmacion(
        string tokenSeguro,
        string accionFormulario)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport"
                  content="width=device-width, initial-scale=1">

            <title>Confirmar cuenta</title>

            <style>
                * {
                    box-sizing: border-box;
                }

                body {
                    margin: 0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 20px;
                    background: #f0f2f5;
                    color: #333333;
                    font-family: "Segoe UI", Arial, sans-serif;
                }

                .contenedor {
                    width: 100%;
                    max-width: 500px;
                    overflow: hidden;
                    background: #ffffff;
                    border-radius: 12px;
                    box-shadow:
                        0 4px 20px rgba(0, 51, 102, 0.15);
                }

                .encabezado {
                    padding: 25px 30px;
                    background: #003366;
                    color: #ffffff;
                }

                .encabezado h1 {
                    margin: 0;
                    font-size: 25px;
                }

                .contenido {
                    padding: 30px;
                    text-align: center;
                }

                .contenido p {
                    margin: 0 0 24px;
                    color: #555555;
                    line-height: 1.6;
                }

                button {
                    min-height: 44px;
                    padding: 11px 24px;
                    border: none;
                    border-radius: 7px;
                    background: #003366;
                    color: #ffffff;
                    font-family: inherit;
                    font-size: 15px;
                    font-weight: 600;
                    cursor: pointer;
                }

                button:hover {
                    background: #001a33;
                }
            </style>
        </head>

        <body>
            <main class="contenedor">
                <header class="encabezado">
                    <h1>Confirmar cuenta</h1>
                </header>

                <section class="contenido">
                    <p>
                        Presione el botón para confirmar su cuenta
                        de Carnet Digital.
                    </p>

                        <form method="post"
                              action="{{accionFormulario}}">
                            <input type="hidden"
                                   name="token"
                                   value="{{tokenSeguro}}">

                            <button type="submit">
                                Confirmar cuenta
                            </button>
                        </form>
                </section>
            </main>
        </body>
        </html>
        """;
    }

    private static string CrearPaginaResultado(
        string titulo,
        string mensaje,
        bool exito)
    {
        var tituloSeguro =
            WebUtility.HtmlEncode(titulo);

        var mensajeSeguro =
            WebUtility.HtmlEncode(mensaje);

        var icono =
            exito ? "✓" : "!";

        var clase =
            exito ? "exito" : "error";

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport"
                  content="width=device-width, initial-scale=1">

            <title>{{tituloSeguro}}</title>

            <style>
                * {
                    box-sizing: border-box;
                }

                body {
                    margin: 0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 20px;
                    background: #f0f2f5;
                    color: #333333;
                    font-family: "Segoe UI", Arial, sans-serif;
                }

                .contenedor {
                    width: 100%;
                    max-width: 500px;
                    padding: 35px 30px;
                    background: #ffffff;
                    border-radius: 12px;
                    box-shadow:
                        0 4px 20px rgba(0, 51, 102, 0.15);
                    text-align: center;
                }

                .icono {
                    width: 65px;
                    height: 65px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 20px;
                    border-radius: 50%;
                    color: #ffffff;
                    font-size: 34px;
                    font-weight: bold;
                }

                .icono.exito {
                    background: #28a745;
                }

                .icono.error {
                    background: #dc3545;
                }

                h1 {
                    margin: 0 0 12px;
                    color: #003366;
                    font-size: 25px;
                }

                p {
                    margin: 0;
                    color: #555555;
                    line-height: 1.6;
                }
            </style>
        </head>

        <body>
            <main class="contenedor">
                <div class="icono {{clase}}">
                    {{icono}}
                </div>

                <h1>{{tituloSeguro}}</h1>

                <p>{{mensajeSeguro}}</p>
            </main>
        </body>
        </html>
        """;
    }
}