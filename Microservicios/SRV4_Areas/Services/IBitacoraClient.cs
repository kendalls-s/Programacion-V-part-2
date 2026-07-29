namespace SRV4_Areas.Services;

public interface IBitacoraClient
{
    Task<bool> RegistrarAsync(
        string token,
        string usuario,
        string accion,
        string detalleJson,
        bool esError = false);
}