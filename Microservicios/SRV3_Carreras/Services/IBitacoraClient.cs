namespace SRV3_Carreras.Services;

public interface IBitacoraClient
{
    Task RegistrarAsync(
        string token,
        string usuario,
        string accion,
        string detalleJson,
        bool esError = false);
}