using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.Instituciones
{
    public class IndexModel : PageModel
    {
        private readonly IInstitucionService _institucionService;
        private readonly IConfiguration _configuration;

        public List<Institucion> Instituciones { get; set; } = new();

        public string InstitucionesApiUrl { get; set; } = string.Empty;

        public IndexModel(
            IInstitucionService institucionService,
            IConfiguration configuration)
        {
            _institucionService = institucionService;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            InstitucionesApiUrl =
                _configuration["Services:Instituciones"]
                ?? string.Empty;

            try
            {
                Instituciones =
                    await _institucionService.GetAllAsync();
            }
            catch
            {
                Instituciones = new List<Institucion>();
            }
        }
    }
}