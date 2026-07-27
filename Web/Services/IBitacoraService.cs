using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface IBitacoraService
    {
        Task<BitacoraRespuestaModel> ObtenerConFiltrosAsync(
            string? token,
            DateTime? fecha,
            string? usuario,
            string? accion,
            int pagina,
            int tamanoPagina,
            bool soloErrores);
    }
}