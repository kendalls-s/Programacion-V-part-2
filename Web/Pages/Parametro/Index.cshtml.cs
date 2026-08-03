using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarnetDigitalWeb.Services;

namespace CarnetDigitalWeb.Pages.Parametro
{
    public class IndexModel : PageModel
    {
        private readonly IParametroService _parametroService;
        private readonly ILogger<IndexModel> _logger;

        public List<CarnetDigitalWeb.Models.Parametro> Items { get; set; } = new();
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int TamanoPagina { get; set; } = 10;

        public IndexModel(IParametroService parametroService, ILogger<IndexModel> logger)
        {
            _parametroService = parametroService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int pagina = 1)
        {
            try
            {
                Pagina = pagina;

                var token = HttpContext.Session.GetString("Token");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No hay token en sesión para parámetros");
                    return RedirectToPage("/Login");
                }

                _logger.LogInformation("📡 Obteniendo parámetros con token");

                var (ok, error, data) = await _parametroService.GetAllAsync(token);

                if (!ok)
                {
                    _logger.LogError("❌ Error al obtener parámetros: {Error}", error);
                    TempData["Error"] = error ?? "Error al cargar parámetros";
                    return Page();
                }

                // ✅ CORREGIDO: Usar el nombre completo
                var lista = data ?? new List<CarnetDigitalWeb.Models.Parametro>();

                TotalItems = lista.Count;
                TotalPaginas = (int)Math.Ceiling((double)TotalItems / TamanoPagina);

                if (TotalPaginas > 0 && Pagina > TotalPaginas)
                {
                    Pagina = TotalPaginas;
                }

                Items = lista
                    .Skip((Pagina - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();

                _logger.LogInformation("✅ {Count} parámetros cargados en página {Pagina}", Items.Count, Pagina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en OnGetAsync");
                TempData["Error"] = "Error al cargar parámetros";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No hay token en sesión para eliminar parámetro");
                    return RedirectToPage("/Login");
                }

                _logger.LogInformation("🗑️ Eliminando parámetro: {Id}", id);

                var (ok, error) = await _parametroService.DeleteAsync(id, token);

                if (!ok)
                {
                    _logger.LogWarning("⚠️ Error al eliminar parámetro {Id}: {Error}", id, error);
                    TempData["Error"] = error ?? "Error al eliminar el parámetro";
                }
                else
                {
                    TempData["Mensaje"] = $"Parámetro '{id}' eliminado correctamente";
                    TempData["MensajeTipo"] = "success";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar parámetro {Id}", id);
                TempData["Error"] = "Error al eliminar el parámetro";
            }

            return RedirectToPage(new { pagina = Pagina });
        }
    }
}