using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages;

public class AutoRegistroModel : PageModel
{
    private readonly ITipoUsuarioService _tipoUsuarioService;
    private readonly ITipoIdentificacionService _tipoIdentificacionService;
    private readonly IInstitucionService _institucionService;
    private readonly ICarreraService _carreraService;
    private readonly IAreaService _areaService;
    private readonly IRolService _rolService;
    private readonly IAutoRegistroService _autoRegistroService;
    private readonly ILogger<AutoRegistroModel> _logger;

    public AutoRegistroModel(
        ITipoUsuarioService tipoUsuarioService,
        ITipoIdentificacionService tipoIdentificacionService,
        IInstitucionService institucionService,
        ICarreraService carreraService,
        IAreaService areaService,
        IRolService rolService,
        IAutoRegistroService autoRegistroService,
        ILogger<AutoRegistroModel> logger)
    {
        _tipoUsuarioService = tipoUsuarioService;
        _tipoIdentificacionService = tipoIdentificacionService;
        _institucionService = institucionService;
        _carreraService = carreraService;
        _areaService = areaService;
        _rolService = rolService;
        _autoRegistroService = autoRegistroService;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetTiposUsuarioAsync()
    {
        try
        {
            var tipos = await _tipoUsuarioService.GetAllAsync();

            var resultado = tipos
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.Id,
                    nombre = x.Nombre
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar los tipos de usuario.");

            return new JsonResult(new
            {
                success = false,
                message = "No se pudieron cargar los tipos de usuario."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnGetTiposIdentificacionAsync()
    {
        try
        {
            var tipos =
                await _tipoIdentificacionService.GetAllAsync();

            var resultado = tipos
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.Id,
                    nombre = x.Nombre
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar los tipos de identificación.");

            return new JsonResult(new
            {
                success = false,
                message =
                    "No se pudieron cargar los tipos de identificación."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnGetInstitucionesAsync()
    {
        try
        {
            var instituciones =
                await _institucionService.GetAllAsync();

            var resultado = instituciones
                .Where(x => x.Activo)
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.ID,
                    nombre = x.Nombre
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar las instituciones.");

            return new JsonResult(new
            {
                success = false,
                message = "No se pudieron cargar las instituciones."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnGetCarrerasAsync(
        int? institucionId)
    {
        try
        {
            var carreras = await _carreraService.GetAllAsync();

            var resultado = carreras
                .Where(x => x.Activo)
                .Where(x =>
                    !institucionId.HasValue ||
                    x.InstitucionID == institucionId.Value)
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.ID,
                    nombre = x.Nombre,
                    institucionId = x.InstitucionID
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar las carreras.");

            return new JsonResult(new
            {
                success = false,
                message = "No se pudieron cargar las carreras."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnGetAreasAsync(
        int? institucionId)
    {
        try
        {
            var areas = await _areaService.GetAllAsync();

            var resultado = areas
                .Where(x => x.Activo)
                .Where(x =>
                    !institucionId.HasValue ||
                    x.InstitucionID == institucionId.Value)
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.ID,
                    nombre = x.Nombre,
                    institucionId = x.InstitucionID
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar las áreas.");

            return new JsonResult(new
            {
                success = false,
                message = "No se pudieron cargar las áreas."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnGetRolesAsync()
    {
        try
        {
            var roles = await _rolService.GetAllAsync();

            var resultado = roles
                .OrderBy(x => x.Nombre)
                .Select(x => new
                {
                    id = x.Id,
                    nombre = x.Nombre
                });

            return new JsonResult(new
            {
                success = true,
                data = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al cargar los roles.");

            return new JsonResult(new
            {
                success = false,
                message = "No se pudieron cargar los roles."
            })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<IActionResult> OnPostRegistrarAsync(
        [FromBody] RegistroUsuarioRequest request)
    {
        try
        {
            var errorValidacion = ValidarRegistro(request);

            if (errorValidacion is not null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = errorValidacion
                });
            }

            var resultado =
                await _autoRegistroService.RegistrarAsync(request);

            if (!resultado.success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = resultado.message
                });
            }

            return new JsonResult(new
            {
                success = true,
                message = resultado.message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al registrar el usuario.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message =
                        "Ocurrió un error inesperado durante el registro."
                });
        }
    }

    private static string? ValidarRegistro(
        RegistroUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
        {
            return "Debe ingresar el nombre completo.";
        }

        if (request.TipoIdentificacionId <= 0)
        {
            return "Debe seleccionar el tipo de identificación.";
        }

        if (string.IsNullOrWhiteSpace(
                request.NumeroIdentificacion))
        {
            return "Debe ingresar el número de identificación.";
        }

        if (request.TipoUsuarioId <= 0)
        {
            return "Debe seleccionar el tipo de usuario.";
        }

        if (request.RolId <= 0)
        {
            return "Debe seleccionar un rol.";
        }

        if (request.Instituciones.Count == 0)
        {
            return "Debe seleccionar una institución.";
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return "Debe ingresar el correo electrónico.";
        }

        if (request.Telefonos.Count == 0 ||
            request.Telefonos.All(string.IsNullOrWhiteSpace))
        {
            return "Debe ingresar al menos un teléfono.";
        }

        if (string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return "Debe ingresar una contraseña.";
        }

        return null;
    }
}