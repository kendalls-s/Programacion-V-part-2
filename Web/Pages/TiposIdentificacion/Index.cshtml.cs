using CarnetDigitalWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.TiposIdentificacion
{
    public class IndexModel : PageModel
    {
        public List<TipoIdentificacion> TiposIdentificacion { get; set; } = new();
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            // Los datos se cargarán desde JavaScript
        }
    }
}