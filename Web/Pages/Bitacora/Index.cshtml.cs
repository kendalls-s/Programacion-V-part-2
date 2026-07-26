using CarnetDigitalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarnetDigitalWeb.Pages.Bitacora
{
    public class IndexModel : PageModel
    {
        private readonly IBitacoraService _bitacoraService;

        public IndexModel(
            IBitacoraService bitacoraService)
        {
            _bitacoraService = bitacoraService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnGetFiltros(
            DateTime? fecha,
            string? usuario,
            string? accion,
            int pagina = 1,
            int tamanoPagina = 15,
            bool soloErrores = false)
        {
            try
            {
                var token =
                    HttpContext.Session.GetString("Token");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new JsonResult(new
                    {
                        mensaje = "La sesión ha expirado. Inicie sesión nuevamente."
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }

                var resultado =
                    await _bitacoraService.ObtenerConFiltrosAsync(
                        token,
                        fecha,
                        usuario,
                        accion,
                        pagina,
                        tamanoPagina,
                        soloErrores
                    );

                return new JsonResult(resultado);
            }
            catch (HttpRequestException ex)
            {
                return new JsonResult(new
                {
                    mensaje = ex.Message
                })
                {
                    StatusCode = StatusCodes.Status502BadGateway
                };
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    mensaje = ex.Message
                })
                {
                    StatusCode =
                        StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}