using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.Qr
{
    public class IndexModel : PageModel
    {
        private readonly ICarnetQRService _service;

        public IndexModel(ICarnetQRService service)
        {
            _service = service;
        }

        [BindProperty]
        public string? Identificacion { get; set; }

        // Base64 del QR devuelto por SRV14 (se muestra en pantalla).
        public string? QrBase64 { get; set; }

        public void OnGet()
        {
        }

        // Todo el consumo del microservicio se hace en el servidor (igual que Instituciones),
        // usando el token guardado en la sesión: no depende de CORS del navegador.
        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrWhiteSpace(token))
            {
                Avisar("Debe iniciar sesión para continuar.", "warning");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Identificacion))
            {
                Avisar("Indique la identificación del usuario.", "warning");
                return Page();
            }

            var (ok, error, qr) = await _service.ObtenerQRAsync(Identificacion.Trim(), token);
            if (!ok)
            {
                Avisar(error!, "danger");
                return Page();
            }

            QrBase64 = qr;
            return Page();
        }

        private void Avisar(string mensaje, string tipo)
        {
            TempData["Mensaje"] = mensaje;
            TempData["MensajeTipo"] = tipo;
        }
    }
}
