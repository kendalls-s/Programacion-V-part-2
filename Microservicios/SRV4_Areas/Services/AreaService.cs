using SRV4_Areas.Entities;
using SRV4_Areas.Repository;

namespace SRV4_Areas.Services;

public class AreaService : IAreaService
{
    private readonly IAreaRepository _repository;
    private readonly IInstitucionClient _institucionClient;
    private readonly ILogger<AreaService> _logger;

    public AreaService(
        IAreaRepository repository,
        IInstitucionClient institucionClient,
        ILogger<AreaService> logger)
    {
        _repository = repository;
        _institucionClient = institucionClient;
        _logger = logger;
    }

    // ==========================================
    // OBTENER TODAS
    // ==========================================
    public async Task<IEnumerable<AreaTrabajo>> GetAll()
    {
        return await _repository.GetAll();
    }

    // ==========================================
    // OBTENER POR ID
    // ==========================================
    public async Task<AreaTrabajo?> GetById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _repository.GetById(id);
    }

    // ==========================================
    // CREAR
    // ==========================================
    public async Task<(bool success, string message, int? id)> Create(
        CreateAreaRequest request,
        string token)
    {
        try
        {
            if (request == null)
            {
                return (
                    false,
                    "Los datos del área son requeridos",
                    null
                );
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return (
                    false,
                    "El nombre es requerido",
                    null
                );
            }

            if (request.InstitucionID <= 0)
            {
                return (
                    false,
                    "La institución es requerida",
                    null
                );
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return (
                    false,
                    "No se recibió un token válido",
                    null
                );
            }

            /*
             * Validar la institución enviando el mismo JWT
             * recibido por el endpoint de Áreas.
             */
            bool institucionExiste =
                await _institucionClient
                    .ValidateInstitucionExists(
                        request.InstitucionID,
                        token
                    );

            if (!institucionExiste)
            {
                return (
                    false,
                    $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse",
                    null
                );
            }

            /*
             * Obtener la institución para guardar también
             * su nombre dentro del registro del área.
             */
            InstitucionDto? institucion =
                await _institucionClient
                    .GetInstitucionById(
                        request.InstitucionID,
                        token
                    );

            if (institucion == null)
            {
                return (
                    false,
                    $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse",
                    null
                );
            }

            string nombreLimpio =
                request.Nombre.Trim();

            bool nombreExiste =
                await _repository.ExistsByNombre(
                    nombreLimpio
                );

            if (nombreExiste)
            {
                return (
                    false,
                    $"Ya existe un área con el nombre '{nombreLimpio}'",
                    null
                );
            }

            AreaTrabajo area = new AreaTrabajo
            {
                Nombre = nombreLimpio,
                InstitucionID = request.InstitucionID,
                InstitucionNombre = institucion.Nombre,
                Activo = true,
                FechaCreacion = DateTime.Now,
                FechaModificacion = null
            };

            int id =
                await _repository.Create(area);

            if (id <= 0)
            {
                return (
                    false,
                    "No se pudo crear el área",
                    null
                );
            }

            return (
                true,
                "Área creada exitosamente",
                id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al crear el área {Nombre}",
                request?.Nombre
            );

            return (
                false,
                $"Error al crear el área: {ex.Message}",
                null
            );
        }
    }

    // ==========================================
    // ACTUALIZAR
    // ==========================================
    public async Task<(bool success, string message)> Update(
        UpdateAreaRequest request,
        string token)
    {
        try
        {
            if (request == null)
            {
                return (
                    false,
                    "Los datos del área son requeridos"
                );
            }

            if (request.ID <= 0)
            {
                return (
                    false,
                    "ID inválido"
                );
            }

            AreaTrabajo? existente =
                await _repository.GetById(request.ID);

            if (existente == null)
            {
                return (
                    false,
                    "Área no encontrada"
                );
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return (
                    false,
                    "El nombre es requerido"
                );
            }

            if (request.InstitucionID <= 0)
            {
                return (
                    false,
                    "La institución es requerida"
                );
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return (
                    false,
                    "No se recibió un token válido"
                );
            }

            bool institucionExiste =
                await _institucionClient
                    .ValidateInstitucionExists(
                        request.InstitucionID,
                        token
                    );

            if (!institucionExiste)
            {
                return (
                    false,
                    $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse"
                );
            }

            InstitucionDto? institucion =
                await _institucionClient
                    .GetInstitucionById(
                        request.InstitucionID,
                        token
                    );

            if (institucion == null)
            {
                return (
                    false,
                    $"La institución con ID {request.InstitucionID} no existe o no pudo consultarse"
                );
            }

            string nombreLimpio =
                request.Nombre.Trim();

            bool nombreDuplicado =
                await _repository.ExistsByNombre(
                    nombreLimpio,
                    request.ID
                );

            if (nombreDuplicado)
            {
                return (
                    false,
                    $"Ya existe otra área con el nombre '{nombreLimpio}'"
                );
            }

            existente.Nombre = nombreLimpio;
            existente.InstitucionID = request.InstitucionID;
            existente.InstitucionNombre = institucion.Nombre;
            existente.FechaModificacion = DateTime.Now;

            bool actualizado =
                await _repository.Update(existente);

            if (!actualizado)
            {
                return (
                    false,
                    "No se pudo actualizar el área"
                );
            }

            return (
                true,
                "Área actualizada exitosamente"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al actualizar el área con ID {Id}",
                request?.ID
            );

            return (
                false,
                $"Error al actualizar el área: {ex.Message}"
            );
        }
    }

    // ==========================================
    // ELIMINAR
    // ==========================================
    public async Task<(bool success, string message)> Delete(
        int id)
    {
        try
        {
            if (id <= 0)
            {
                return (
                    false,
                    "ID inválido"
                );
            }

            AreaTrabajo? existente =
                await _repository.GetById(id);

            if (existente == null)
            {
                return (
                    false,
                    "Área no encontrada"
                );
            }

            bool eliminada =
                await _repository.Delete(id);

            if (!eliminada)
            {
                return (
                    false,
                    "No se pudo eliminar el área"
                );
            }

            return (
                true,
                "Área eliminada exitosamente"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al eliminar el área con ID {Id}",
                id
            );

            return (
                false,
                $"Error al eliminar el área: {ex.Message}"
            );
        }
    }
}