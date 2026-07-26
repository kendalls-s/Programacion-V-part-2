using BitacoraSRV9.Auth;
using BitacoraSRV9.Entities;
using BitacoraSRV9.Helpers;
using BitacoraSRV9.Services;

namespace BitacoraSRV9;

public static class BitacoraEndpoints
{
    public static void MapBitacoraEndpoints(
        this IEndpointRouteBuilder routes)
    {
        // ========================================
        // POST - Registrar en bitácora
        // ========================================

        routes.MapPost("/bitacora",
        async (
            HttpContext http,
            BitacoraRequest request,
            IBitacoraService service,
            ITokenValidator tokenValidator) =>
        {
            var token = ObtenerToken(http);

            if (!await tokenValidator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var resultado =
                    await service.RegistrarAsync(request);

                if (!resultado.ok)
                {
                    await service.RegistrarErrorAsync(
                        request.Usuario,
                        "Error al registrar en bitácora",
                        BitacoraHelper.CrearJsonError(
                            resultado.error
                        )
                    );

                    return Results.BadRequest(new
                    {
                        mensaje = resultado.error,
                        usuario = request.Usuario,
                        accion = request.Accion
                    });
                }

                return Results.Ok(new
                {
                    mensaje =
                        "Movimiento registrado correctamente",
                    usuario = request.Usuario,
                    accion = request.Accion,
                    fecha = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                await service.RegistrarErrorAsync(
                    request.Usuario,
                    ex,
                    "POST /bitacora"
                );

                return Results.BadRequest(new
                {
                    mensaje =
                        "Error al registrar en bitácora",
                    error = ex.Message
                });
            }
        });

        // ========================================
        // GET - Obtener con filtros y paginación
        // ========================================

        routes.MapGet("/bitacora/filtros",
        async (
            HttpContext http,
            [AsParameters] BitacoraFiltrosRequest filtros,
            IBitacoraService service,
            ITokenValidator tokenValidator) =>
        {
            var token = ObtenerToken(http);

            if (!await tokenValidator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var resultado =
                    await service.ObtenerConFiltrosAsync(
                        filtros
                    );

                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                await service.RegistrarErrorAsync(
                    "Sistema",
                    ex,
                    "GET /bitacora/filtros"
                );

                return Results.BadRequest(new
                {
                    mensaje =
                        "Error al obtener bitácora con filtros",
                    error = ex.Message
                });
            }
        });

        // ========================================
        // GET - Obtener todos
        // ========================================

        routes.MapGet("/bitacora",
        async (
            HttpContext http,
            IBitacoraService service,
            ITokenValidator tokenValidator) =>
        {
            var token = ObtenerToken(http);

            if (!await tokenValidator.ValidateAsync(token))
            {
                return Results.Unauthorized();
            }

            try
            {
                var lista =
                    await service.ObtenerTodosAsync();

                return Results.Ok(lista);
            }
            catch (Exception ex)
            {
                await service.RegistrarErrorAsync(
                    "Sistema",
                    ex,
                    "GET /bitacora"
                );

                return Results.BadRequest(new
                {
                    mensaje =
                        "Error al obtener bitácora",
                    error = ex.Message
                });
            }
        });
    }

    private static string ObtenerToken(
        HttpContext http)
    {
        var authorization =
            http.Request.Headers.Authorization
                .ToString();

        if (string.IsNullOrWhiteSpace(authorization))
        {
            return string.Empty;
        }

        const string bearer = "Bearer ";

        if (!authorization.StartsWith(
            bearer,
            StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return authorization[bearer.Length..].Trim();
    }
}