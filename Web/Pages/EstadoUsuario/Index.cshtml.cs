using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.EstadoUsuario
{
    public class IndexModel : PageModel
    {
        private readonly IEstadoUsuarioService _service;

        public IndexModel(IEstadoUsuarioService service)
        {
            _service = service;
        }

        [BindProperty]
        public int? UsuarioId { get; set; }

        [BindProperty]
        public string? Estado { get; set; }

        // Resultado devuelto por SRV12 (se muestra en pantalla).
        public UsuarioEstado? Resultado { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                Avisar("Debe iniciar sesión para continuar.", "warning");
                return Page();
            }

            if (!UsuarioId.HasValue || string.IsNullOrWhiteSpace(Estado))
            {
                Avisar("El identificador del usuario y el código de estado son requeridos.", "warning");
                return Page();
            }

            var (ok, error, data) = await _service.CambiarEstadoAsync(UsuarioId.Value, Estado.Trim(), token);
            if (!ok)
            {
                Avisar(error!, "danger");
                return Page();
            }

            Resultado = data;
            Avisar("El estado se actualizó correctamente.", "success");
            return Page();
        }

        private void Avisar(string mensaje, string tipo)
        {
            TempData["Mensaje"] = mensaje;
            TempData["MensajeTipo"] = tipo;
        }
    }
}
