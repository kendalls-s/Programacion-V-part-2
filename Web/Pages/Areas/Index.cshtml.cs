using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.Areas
{
    public class IndexModel : PageModel
    {
        private readonly IAreaService _areaService;
        private readonly IInstitucionService _institucionService;
        private readonly ILogger<IndexModel> _logger;

        public List<Area> Areas { get; set; } = new();
        public List<Institucion> Instituciones { get; set; } = new();
        public string? MensajeError { get; set; }

        public IndexModel(
            IAreaService areaService,
            IInstitucionService institucionService,
            ILogger<IndexModel> logger)
        {
            _areaService = areaService;
            _institucionService = institucionService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Cargando página de Áreas...");

                // Cargar instituciones para el select
                Instituciones = await _institucionService.GetAllAsync();
                _logger.LogInformation($"Se cargaron {Instituciones.Count} instituciones");

                // Cargar áreas
                Areas = await _areaService.GetAllAsync();
                _logger.LogInformation($"Se cargaron {Areas.Count} áreas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la página de Áreas");
                MensajeError = $"Error al cargar la página: {ex.Message}";
            }
        }

        // Handler para obtener todas las áreas (llamado desde JavaScript)
        public async Task<IActionResult> OnGetGetAllAsync()
        {
            try
            {
                var areas = await _areaService.GetAllAsync();
                return new JsonResult(areas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las áreas");
                return new JsonResult(new { error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // Handler para obtener instituciones (llamado desde JavaScript)
        public async Task<IActionResult> OnGetInstitucionesAsync()
        {
            try
            {
                var instituciones = await _institucionService.GetAllAsync();
                return new JsonResult(instituciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las instituciones");
                return new JsonResult(new { error = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        // Handler para buscar área por ID
        public async Task<IActionResult> OnGetBuscarAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "El ID debe ser mayor que cero."
                })
                {
                    StatusCode = 400
                };
            }

            var area = await _areaService.GetByIdAsync(id);

            if (area == null)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = $"No se encontró un área con ID {id}."
                })
                {
                    StatusCode = 404
                };
            }

            return new JsonResult(new
            {
                exito = true,
                data = area
            });
        }

        // Handler para crear área
        public async Task<IActionResult> OnPostCrearAsync([FromBody] AreaRequest request)
        {
            try
            {
                var area = new Area
                {
                    Nombre = request.Nombre,
                    InstitucionID = request.InstitucionID,
                    Activo = request.Activo
                };

                var (success, message, id) = await _areaService.CreateAsync(area);

                return new JsonResult(new
                {
                    exito = success,
                    mensaje = message,
                    id = id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear área");
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = $"Error al crear el área: {ex.Message}"
                })
                {
                    StatusCode = 500
                };
            }
        }

        // Handler para editar área
        public async Task<IActionResult> OnPostEditarAsync(int id, [FromBody] AreaRequest request)
        {
            try
            {
                var area = new Area
                {
                    ID = id,
                    Nombre = request.Nombre,
                    InstitucionID = request.InstitucionID,
                    Activo = request.Activo
                };

                var (success, message) = await _areaService.UpdateAsync(id, area);

                return new JsonResult(new
                {
                    exito = success,
                    mensaje = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar área");
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = $"Error al editar el área: {ex.Message}"
                })
                {
                    StatusCode = 500
                };
            }
        }

        // Handler para eliminar área
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var (success, message) = await _areaService.DeleteAsync(id);

                return new JsonResult(new
                {
                    exito = success,
                    mensaje = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar área");
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = $"Error al eliminar el área: {ex.Message}"
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}