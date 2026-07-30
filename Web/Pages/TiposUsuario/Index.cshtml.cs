using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;

namespace CarnetDigitalWeb.Pages.TiposUsuario
{
    public class IndexModel : PageModel
    {
        private readonly ITipoUsuarioService _service;
        private readonly ILogger<IndexModel> _logger;

        public List<TipoUsuario> TiposUsuario { get; set; } = new();

        public IndexModel(ITipoUsuarioService service, ILogger<IndexModel> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ✅ VALIDACIÓN DE TOKEN AGREGADA
        public async Task<IActionResult> OnGetAsync()
        {
            // ✅ Verificar si hay token en la sesión
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                // ✅ Redirigir al Login si no hay token
                return RedirectToPage("/Login");
            }

            try
            {
                TiposUsuario = await _service.GetAllAsync();
                _logger.LogInformation($"Tipos de usuario cargados: {TiposUsuario.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tipos de usuario");
                TiposUsuario = new List<TipoUsuario>();
            }

            return Page();
        }
    }
}