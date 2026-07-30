using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;

namespace CarnetDigitalWeb.Pages.TiposIdentificacion
{
    public class IndexModel : PageModel
    {
        private readonly ITipoIdentificacionService _service;
        private readonly ILogger<IndexModel> _logger;

        public List<TipoIdentificacion> TiposIdentificacion { get; set; } = new();

        public IndexModel(ITipoIdentificacionService service, ILogger<IndexModel> logger)
        {
            _service = service;
            _logger = logger;
        }

        //  VALIDACIÓN DE TOKEN
        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                TiposIdentificacion = await _service.GetAllAsync();
                _logger.LogInformation($"Tipos de identificación cargados: {TiposIdentificacion.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tipos de identificación");
                TiposIdentificacion = new List<TipoIdentificacion>();
            }

            return Page();
        }
    }
}