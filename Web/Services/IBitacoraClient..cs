using System.Threading.Tasks;

namespace SRV2_Instituciones.Services
{
    public interface IBitacoraClient
    {
        Task RegistrarAsync(
            string usuario,
            string accion,
            string detalleJson);
    }
}