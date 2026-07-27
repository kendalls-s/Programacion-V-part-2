using CarnetDigitalWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.TiposUsuario
{
    public class IndexModel : PageModel
    {
        public List<TipoUsuario> TiposUsuario { get; set; } = new();
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            // Los datos se cargarán desde JavaScript
        }
    }
}