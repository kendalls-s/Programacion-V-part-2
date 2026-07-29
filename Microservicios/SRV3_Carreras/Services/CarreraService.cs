using SRV3_Carreras.Entities;
using SRV3_Carreras.Repository;
using System.Text.RegularExpressions;

namespace SRV3_Carreras.Services;

public class CarreraService : ICarreraService
{
    private readonly ICarreraRepository _repository;
    private readonly IInstitucionClient _institucionClient;
    private readonly ILogger<CarreraService> _logger;

    public CarreraService(
        ICarreraRepository repository,
        IInstitucionClient institucionClient,
        ILogger<CarreraService> logger)
    {
        _repository = repository;
        _institucionClient = institucionClient;
        _logger = logger;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        email = email.Trim().ToLower();

        bool esValido = email.EndsWith("@cuc.ac.cr") || email.EndsWith("@cuc.cr");

        if (esValido)
        {
            if (email.EndsWith("@cuc.ac.cr"))
            {
                var localPart = email.Substring(0, email.Length - "@cuc.ac.cr".Length);
                return !string.IsNullOrWhiteSpace(localPart) && !localPart.Contains(" ");
            }

            if (email.EndsWith("@cuc.cr"))
            {
                var localPart = email.Substring(0, email.Length - "@cuc.cr".Length);
                return !string.IsNullOrWhiteSpace(localPart) && !localPart.Contains(" ");
            }
        }

        return false;
    }

    private bool IsValidPhone(string telefono)
    {
        return !string.IsNullOrWhiteSpace(telefono) && Regex.IsMatch(telefono, @"^\d+$");
    }

    public async Task<IEnumerable<Carrera>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<Carrera?> GetById(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetById(id);
    }

    public async Task<(bool success, string message, int? id)> Create(
        CreateCarreraRequest request,
        string token)
    {
        try
        {
            if (request == null)
                return (false, "Los datos de la carrera son requeridos", null);

            if (string.IsNullOrWhiteSpace(request.Nombre))
                return (false, "El nombre es requerido", null);

            if (string.IsNullOrWhiteSpace(request.Director))
                return (false, "El director es requerido", null);

            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "El email es requerido", null);

            if (!IsValidEmail(request.Email))
                return (false, "El email no es válido. Solo se permiten correos @cuc.ac.cr o @cuc.cr", null);

            if (string.IsNullOrWhiteSpace(request.Telefono) || !IsValidPhone(request.Telefono))
                return (false, "El teléfono solo debe contener números", null);

            if (request.InstitucionID <= 0)
                return (false, "La institución es requerida", null);

            if (string.IsNullOrWhiteSpace(token))
                return (false, "No se recibió un token válido", null);

            // Validar que la institución existe
            bool institucionExiste = await _institucionClient.ValidateInstitucionExists(
                request.InstitucionID,
                token);

            if (!institucionExiste)
                return (false, $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse", null);

            // Obtener el nombre de la institución
            InstitucionDto? institucion = await _institucionClient.GetInstitucionById(
                request.InstitucionID,
                token);

            if (institucion == null)
                return (false, $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse", null);

            if (await _repository.ExistsByNombre(request.Nombre))
                return (false, $"Ya existe una carrera con el nombre '{request.Nombre}'", null);

            var carrera = new Carrera
            {
                Nombre = request.Nombre.Trim(),
                Director = request.Director.Trim(),
                Email = request.Email.Trim().ToLower(),
                Telefono = request.Telefono.Trim(),
                InstitucionID = request.InstitucionID,
                InstitucionNombre = institucion.Nombre,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            int id = await _repository.Create(carrera);
            return (true, "Carrera creada exitosamente", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la carrera {Nombre}", request?.Nombre);
            return (false, $"Error al crear la carrera: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> Update(
        UpdateCarreraRequest request,
        string token)
    {
        try
        {
            if (request == null)
                return (false, "Los datos de la carrera son requeridos");

            if (request.ID <= 0)
                return (false, "ID inválido");

            var existing = await _repository.GetById(request.ID);
            if (existing == null)
                return (false, "Carrera no encontrada");

            if (string.IsNullOrWhiteSpace(request.Nombre))
                return (false, "El nombre es requerido");

            if (string.IsNullOrWhiteSpace(request.Director))
                return (false, "El director es requerido");

            if (string.IsNullOrWhiteSpace(request.Email))
                return (false, "El email es requerido");

            if (!IsValidEmail(request.Email))
                return (false, "El email no es válido. Solo se permiten correos @cuc.ac.cr o @cuc.cr");

            if (string.IsNullOrWhiteSpace(request.Telefono) || !IsValidPhone(request.Telefono))
                return (false, "El teléfono solo debe contener números");

            if (request.InstitucionID <= 0)
                return (false, "La institución es requerida");

            if (string.IsNullOrWhiteSpace(token))
                return (false, "No se recibió un token válido");

            // Validar que la institución existe
            bool institucionExiste = await _institucionClient.ValidateInstitucionExists(
                request.InstitucionID,
                token);

            if (!institucionExiste)
                return (false, $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse");

            // Obtener el nombre de la institución
            InstitucionDto? institucion = await _institucionClient.GetInstitucionById(
                request.InstitucionID,
                token);

            if (institucion == null)
                return (false, $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse");

            if (await _repository.ExistsByNombre(request.Nombre, request.ID))
                return (false, $"Ya existe otra carrera con el nombre '{request.Nombre}'");

            existing.Nombre = request.Nombre.Trim();
            existing.Director = request.Director.Trim();
            existing.Email = request.Email.Trim().ToLower();
            existing.Telefono = request.Telefono.Trim();
            existing.InstitucionID = request.InstitucionID;
            existing.InstitucionNombre = institucion.Nombre;
            existing.FechaModificacion = DateTime.Now;

            bool updated = await _repository.Update(existing);
            return updated ? (true, "Carrera actualizada exitosamente") : (false, "No se pudo actualizar la carrera");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la carrera con ID {Id}", request?.ID);
            return (false, $"Error al actualizar la carrera: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> Delete(int id)
    {
        try
        {
            if (id <= 0)
                return (false, "ID inválido");

            var existing = await _repository.GetById(id);
            if (existing == null)
                return (false, "Carrera no encontrada");

            bool deleted = await _repository.Delete(id);
            return deleted ? (true, "Carrera eliminada exitosamente") : (false, "No se pudo eliminar la carrera");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la carrera con ID {Id}", id);
            return (false, $"Error al eliminar la carrera: {ex.Message}");
        }
    }
}