using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services;

public interface IAutoRegistroService
{
    Task<(bool success, string message)> RegistrarAsync(
        RegistroUsuarioRequest request);
}