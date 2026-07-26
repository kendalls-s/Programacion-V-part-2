using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolService _rolService;

        public IndexModel(IRolService rolService)
        {
            _rolService = rolService;
        }

        public List<Rol> Roles { get; set; } = new();

        public List<PantallaRol> PantallasDisponibles { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Roles = await _rolService.GetAllAsync();

                PantallasDisponibles = Roles
                    .Where(rol => rol.Pantallas != null)
                    .SelectMany(rol => rol.Pantallas)
                    .Where(pantalla => pantalla.Id > 0)
                    .GroupBy(pantalla => pantalla.Id)
                    .Select(grupo => grupo.First())
                    .OrderBy(pantalla => pantalla.Id)
                    .ToList();

                if (Roles.Count == 0)
                {
                    MensajeError =
                        "No se encontraron roles o no fue posible consultar el servicio.";
                }
            }
            catch (Exception ex)
            {
                MensajeError =
                    $"Error al cargar los roles: {ex.Message}";
            }
        }

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

            var rol = await _rolService.GetByIdAsync(id);

            if (rol == null)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = $"No se encontró un rol con ID {id}."
                })
                {
                    StatusCode = 404
                };
            }

            return new JsonResult(new
            {
                exito = true,
                data = rol
            });
        }

        public async Task<IActionResult> OnPostCrearAsync(
            [FromBody] RolRequest request)
        {
            var mensajeValidacion = ValidarRequest(request);

            if (mensajeValidacion != null)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = mensajeValidacion
                })
                {
                    StatusCode = 400
                };
            }

            var creado = await _rolService.CreateAsync(request);

            return new JsonResult(new
            {
                exito = creado,
                mensaje = creado
                    ? "Rol creado correctamente."
                    : "No fue posible crear el rol."
            })
            {
                StatusCode = creado ? 200 : 400
            };
        }

        public async Task<IActionResult> OnPostEditarAsync(
            int id,
            [FromBody] RolRequest request)
        {
            if (id <= 0)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "El ID del rol no es válido."
                })
                {
                    StatusCode = 400
                };
            }

            var mensajeValidacion = ValidarRequest(request);

            if (mensajeValidacion != null)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = mensajeValidacion
                })
                {
                    StatusCode = 400
                };
            }

            var actualizado =
                await _rolService.UpdateAsync(id, request);

            return new JsonResult(new
            {
                exito = actualizado,
                mensaje = actualizado
                    ? "Rol actualizado correctamente."
                    : "No fue posible actualizar el rol."
            })
            {
                StatusCode = actualizado ? 200 : 400
            };
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new
                {
                    exito = false,
                    mensaje = "El ID del rol no es válido."
                })
                {
                    StatusCode = 400
                };
            }

            var eliminado = await _rolService.DeleteAsync(id);

            return new JsonResult(new
            {
                exito = eliminado,
                mensaje = eliminado
                    ? "Rol eliminado correctamente."
                    : "No fue posible eliminar el rol."
            })
            {
                StatusCode = eliminado ? 200 : 400
            };
        }

        private static string? ValidarRequest(RolRequest? request)
        {
            if (request == null)
            {
                return "No se recibieron los datos del rol.";
            }

            request.Nombre =
                request.Nombre?.Trim() ?? string.Empty;

            request.Pantallas ??= new List<int>();

            request.Pantallas = request.Pantallas
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return "El nombre del rol es obligatorio.";
            }

            if (request.Nombre.Length > 100)
            {
                return "El nombre no puede superar los 100 caracteres.";
            }

            if (request.Pantallas.Count == 0)
            {
                return "Debe seleccionar al menos una pantalla.";
            }

            return null;
        }
    }
}