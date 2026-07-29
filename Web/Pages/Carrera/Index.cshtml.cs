using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;

namespace CarnetDigitalWeb.Pages.Carrera
{
    public class IndexModel : PageModel
    {
        private readonly ICarreraService _carreraService;
        private readonly IInstitucionService _institucionService;

        public List<Models.Carrera> Carreras { get; set; } = new();
        public List<Institucion> Instituciones { get; set; } = new();
        public string? MensajeError { get; set; }

        public IndexModel(
            ICarreraService carreraService,
            IInstitucionService institucionService)
        {
            _carreraService = carreraService;
            _institucionService = institucionService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var carrerasTask = _carreraService.GetAllAsync();
                var institucionesTask = _institucionService.GetAllAsync();

                await Task.WhenAll(carrerasTask, institucionesTask);

                Carreras = carrerasTask.Result ?? new List<Models.Carrera>();
                Instituciones = institucionesTask.Result ?? new List<Institucion>();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar datos: {ex.Message}";
                Carreras = new List<Models.Carrera>();
                Instituciones = new List<Institucion>();
            }
        }

        // ========================================
        // HANDLERS PARA EL JAVASCRIPT
        // ========================================

        public async Task<IActionResult> OnGetGetAllAsync()
        {
            try
            {
                var carreras = await _carreraService.GetAllAsync();
                return new JsonResult(carreras ?? new List<Models.Carrera>());
            }
            catch
            {
                return new JsonResult(new List<Models.Carrera>());
            }
        }

        public async Task<IActionResult> OnGetInstitucionesAsync()
        {
            try
            {
                var instituciones = await _institucionService.GetAllAsync();
                return new JsonResult(instituciones ?? new List<Institucion>());
            }
            catch
            {
                return new JsonResult(new List<Institucion>());
            }
        }

        public async Task<IActionResult> OnGetBuscarAsync(int id)
        {
            try
            {
                var carrera = await _carreraService.GetByIdAsync(id);
                if (carrera == null)
                {
                    return NotFound();
                }
                return new JsonResult(new { data = carrera, exito = true });
            }
            catch
            {
                return new JsonResult(new { exito = false, mensaje = "Error al buscar la carrera" });
            }
        }

        public async Task<IActionResult> OnPostCrearAsync([FromBody] CreateCarreraRequest request)
        {
            try
            {
                var result = await _carreraService.CreateAsync(request);
                if (result.success)
                {
                    return new JsonResult(new { exito = true, mensaje = result.message, id = result.id });
                }
                return new JsonResult(new { exito = false, mensaje = result.message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEditarAsync(int id, [FromBody] UpdateCarreraRequest request)
        {
            try
            {
                if (id != request.ID)
                {
                    return new JsonResult(new { exito = false, mensaje = "El ID no coincide" });
                }

                var result = await _carreraService.UpdateAsync(request);
                if (result.success)
                {
                    return new JsonResult(new { exito = true, mensaje = result.message });
                }
                return new JsonResult(new { exito = false, mensaje = result.message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var result = await _carreraService.DeleteAsync(id);
                if (result.success)
                {
                    return new JsonResult(new { exito = true, mensaje = result.message });
                }
                return new JsonResult(new { exito = false, mensaje = result.message });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message });
            }
        }
    }
}