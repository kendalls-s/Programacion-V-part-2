using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Data;
using UsuariosSRV4.DTOs;
using UsuariosSRV4.Entities;

namespace UsuariosSRV4.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuarioService> _logger;
        private readonly IAreaApiClient _areaClient;
        private readonly ICarreraApiClient _carreraClient;
        private readonly IInstitucionApiClient _institucionClient;

        public UsuarioService(
            ApplicationDbContext context,
            ILogger<UsuarioService> logger,
            IAreaApiClient areaClient,
            ICarreraApiClient carreraClient,
            IInstitucionApiClient institucionClient)
        {
            _context = context;
            _logger = logger;
            _areaClient = areaClient;
            _carreraClient = carreraClient;
            _institucionClient = institucionClient;
        }

        // ============================================================
        // ✅ VALIDAR CREDENCIALES CON VALIDACIÓN DE CONFIRMACIÓN
        // ============================================================
        public async Task<(bool ok, string? error, ValidarCredencialesResponse? data)> ValidarCredencialesAsync(
            string email, string password, string? tipo = null)
        {
            try
            {
                _logger.LogInformation($"=== VALIDANDO CREDENCIALES: {email} ===");

                var usuario = await _context.Usuarios
                    .Include(u => u.TipoUsuario)
                    .Include(u => u.Estado)
                    .FirstOrDefaultAsync(u => u.Email == email);

                // ❌ Usuario no existe
                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {email}");
                    return (false, "Usuario y/o contraseña incorrectos", null);
                }

                _logger.LogInformation($"Usuario: {email}, Confirmado: {usuario.Confirmado}, IntentosFallidos: {usuario.IntentosFallidos}, Bloqueado: {usuario.Bloqueado}");

                // ============================================================
                // 🔐 VALIDACIÓN DE CONFIRMACIÓN - CRUCIAL
                // ============================================================
                if (!usuario.Confirmado)
                {
                    _logger.LogWarning($"Usuario NO confirmado: {email}");
                    return (false, "Por favor, confirme su cuenta antes de iniciar sesión. Revise su correo electrónico.", null);
                }

                // ✅ VERIFICAR BLOQUEO
                if (usuario.Bloqueado)
                {
                    _logger.LogWarning($"Usuario BLOQUEADO: {email}");
                    return (false, "Usuario bloqueado por intentos fallidos. Contacte al administrador.", null);
                }

                // ✅ VERIFICAR CONTRASEÑA
                if (usuario.Contrasena != password)
                {
                    usuario.IntentosFallidos++;
                    _logger.LogWarning($"Contraseña incorrecta, IntentosFallidos: {usuario.IntentosFallidos}");

                    if (usuario.IntentosFallidos >= 3)
                    {
                        usuario.Bloqueado = true;
                        usuario.FechaBloqueo = DateTime.Now;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning($"Usuario BLOQUEADO por 3 intentos fallidos: {email}");
                        return (false, "Usuario y/o contraseña incorrectos", null);
                    }

                    await _context.SaveChangesAsync();
                    return (false, "Usuario y/o contraseña incorrectos", null);
                }

                // ✅ CONTRASEÑA CORRECTA - REINICIAR INTENTOS
                if (usuario.IntentosFallidos > 0)
                {
                    usuario.IntentosFallidos = 0;
                    usuario.Bloqueado = false;
                    usuario.FechaBloqueo = null;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Intentos reiniciados para: {email}");
                }

                // ✅ VERIFICAR ESTADO (1 = Activo)
                if (usuario.EstadoId != 1)
                {
                    return (false, "Usuario y/o contraseña incorrectos", null);
                }

                // ✅ VALIDAR TIPO DE USUARIO
                if (!string.IsNullOrEmpty(tipo))
                {
                    var tipoUsuarioReal = usuario.TipoUsuario != null ? usuario.TipoUsuario.Nombre : "";

                    _logger.LogInformation($"Tipo seleccionado: {tipo}, Tipo real del usuario: {tipoUsuarioReal}");

                    if (!string.Equals(tipoUsuarioReal, tipo, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning($"❌ Tipo de usuario NO coincide. Esperado: {tipo}, Real: {tipoUsuarioReal}");
                        return (false, "Usuario y/o contraseña incorrectos", null);
                    }

                    _logger.LogInformation($"✅ Tipo de usuario validado correctamente: {tipo}");
                }

                // ✅ TODAS LAS VALIDACIONES PASARON
                var result = new ValidarCredencialesResponse
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    TipoUsuario = usuario.TipoUsuario != null ? usuario.TipoUsuario.Nombre : "",
                    Activo = usuario.EstadoId == 1,
                    Bloqueado = usuario.Bloqueado,
                    IntentosFallidos = usuario.IntentosFallidos,
                    TipoUsuarioId = usuario.TipoUsuarioId,
                    RolId = usuario.RolId
                };

                _logger.LogInformation($"✅ Credenciales válidas para: {email}");
                return (true, null, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ValidarCredencialesAsync: {email}");
                return (false, "Usuario y/o contraseña incorrectos", null);
            }
        }

        // ============================================================
        // ✅ OBTENER TODOS LOS USUARIOS
        // ============================================================
        public async Task<(bool ok, string? error, IEnumerable<UsuarioDto>? data)> GetAllAsync()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.TipoUsuario)
                    .Include(u => u.Estado)
                    .Include(u => u.TipoIdentificacion)
                    .Include(u => u.Telefonos)
                    .Include(u => u.Areas)
                    .Include(u => u.Carreras)
                    .Include(u => u.Instituciones)
                    .Where(u => u.EstadoId == 1)
                    .ToListAsync();

                // Se traen los catálogos completos UNA sola vez y se arma un
                // diccionario en memoria, para no golpear los microservicios
                // de Areas/Carreras/Instituciones con una llamada por usuario.
                var (areasPorId, carrerasPorId, institucionesPorId) = await CargarCatalogosAsync();

                var result = usuarios
                    .Select(u => MapearUsuarioDto(u, areasPorId, carrerasPorId, institucionesPorId))
                    .ToList();

                return (true, null, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener usuarios: {ex.Message}", null);
            }
        }

        // ============================================================
        // ✅ OBTENER USUARIO POR ID
        // ============================================================
        public async Task<(bool ok, string? error, UsuarioDto? data)> GetByIdAsync(int id)
        {
            try
            {
                var u = await _context.Usuarios
                    .Include(u => u.TipoUsuario)
                    .Include(u => u.Estado)
                    .Include(u => u.TipoIdentificacion)
                    .Include(u => u.Telefonos)
                    .Include(u => u.Carreras)
                    .Include(u => u.Areas)
                    .Include(u => u.Instituciones)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (u == null)
                {
                    return (false, "Usuario no encontrado", null);
                }

                var result = await MapearUsuarioDtoConsultandoServiciosAsync(u);

                return (true, null, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener usuario: {ex.Message}", null);
            }
        }

        // ============================================================
        // ✅ OBTENER DETALLE DE USUARIO (GET /api/Usuarios/{id})
        // Devuelve SOLO los campos requeridos para la pantalla de detalle:
        // Email, TipoIdentificacion, NumeroIdentificacion, NombreCompleto,
        // Instituciones, TipoUsuario, Carreras, Areas, Telefonos, Rol.
        // ============================================================
        public async Task<(bool ok, string? error, UsuarioDetalleDto? data)> GetDetalleByIdAsync(int id)
        {
            try
            {
                var u = await _context.Usuarios
                    .Include(u => u.TipoUsuario)
                    .Include(u => u.TipoIdentificacion)
                    .Include(u => u.Rol)
                    .Include(u => u.Telefonos)
                    .Include(u => u.Carreras)
                    .Include(u => u.Areas)
                    .Include(u => u.Instituciones)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (u == null)
                {
                    return (false, "Usuario no encontrado", null);
                }

                // Reutiliza la resolución de Areas/Carreras/Instituciones
                // contra los microservicios dueños de esos catálogos.
                var completo = await MapearUsuarioDtoConsultandoServiciosAsync(u);

                var result = new UsuarioDetalleDto
                {
                    Email = completo.Email,
                    TipoIdentificacion = completo.TipoIdentificacion,
                    NumeroIdentificacion = completo.NumeroIdentificacion,
                    NombreCompleto = completo.NombreCompleto,
                    TipoUsuario = completo.TipoUsuario,
                    Rol = u.Rol?.Nombre ?? string.Empty,
                    Telefonos = completo.Telefonos,
                    Instituciones = completo.Instituciones,
                    Areas = completo.Areas,
                    Carreras = completo.Carreras
                };

                return (true, null, result);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener usuario: {ex.Message}", null);
            }
        }

        // ============================================================
        // ✅ CREAR USUARIO
        // ============================================================
        public async Task<(bool ok, string? error, UsuarioDto? data)> CreateAsync(CrearUsuarioDto dto)
        {
            try
            {
                var exists = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                {
                    return (false, "El email ya está registrado", null);
                }

                var usuario = new Usuario
                {
                    Email = dto.Email ?? string.Empty,
                    Contrasena = dto.Contrasena ?? string.Empty,
                    TipoUsuarioId = dto.TipoUsuarioId,
                    EstadoId = 1,
                    NombreCompleto = dto.NombreCompleto ?? string.Empty,
                    TipoIdentificacionId = dto.TipoIdentificacionId,
                    NumeroIdentificacion = dto.NumeroIdentificacion ?? string.Empty,
                    RolId = dto.RolId ?? 1,
                    Confirmado = dto.Confirmado,
                    FechaCreacion = DateTime.Now,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Fotografia = null
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                AgregarTelefonos(usuario.Id, dto.Telefonos);
                AgregarRelaciones(usuario.Id, dto.InstitucionId, dto.AreaId, dto.CarreraId);

                await _context.SaveChangesAsync();

                var (ok, _, data) = await GetByIdAsync(usuario.Id);
                return (ok, null, data);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear usuario: {ex.Message}", null);
            }
        }

        // ============================================================
        // ✅ ACTUALIZAR USUARIO
        // ============================================================
        public async Task<(bool ok, string? error, UsuarioDto? data)> UpdateAsync(int id, ActualizarUsuarioDto dto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Telefonos)
                    .Include(u => u.Areas)
                    .Include(u => u.Carreras)
                    .Include(u => u.Instituciones)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return (false, "Usuario no encontrado", null);
                }

                usuario.Email = dto.Email ?? string.Empty;
                usuario.TipoIdentificacionId = dto.TipoIdentificacionId;
                usuario.NumeroIdentificacion = dto.NumeroIdentificacion ?? string.Empty;
                usuario.NombreCompleto = dto.NombreCompleto ?? string.Empty;
                usuario.TipoUsuarioId = dto.TipoUsuarioId;
                usuario.EstadoId = dto.EstadoId ?? (dto.Activo ? 1 : 2);
                usuario.Confirmado = dto.Confirmado ?? usuario.Confirmado;
                usuario.RolId = dto.RolId ?? usuario.RolId;

                if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                {
                    usuario.Contrasena = dto.Contrasena ?? string.Empty;
                }

                // Teléfonos: se reemplaza la lista completa
                foreach (var tel in usuario.Telefonos.ToList())
                {
                    _context.UsuariosTelefonos.Remove(tel);
                }
                AgregarTelefonos(usuario.Id, dto.Telefonos);

                // Institución / Área / Carrera: solo se tocan si vinieron en el DTO,
                // para no perder la relación existente en un PUT parcial.
                if (dto.InstitucionId != null)
                {
                    foreach (var rel in usuario.Instituciones.ToList())
                        _context.UsuariosInstituciones.Remove(rel);
                }
                if (dto.AreaId != null)
                {
                    foreach (var rel in usuario.Areas.ToList())
                        _context.UsuariosAreas.Remove(rel);
                }
                if (dto.CarreraId != null)
                {
                    foreach (var rel in usuario.Carreras.ToList())
                        _context.UsuariosCarreras.Remove(rel);
                }
                AgregarRelaciones(usuario.Id, dto.InstitucionId, dto.AreaId, dto.CarreraId);

                await _context.SaveChangesAsync();

                var (ok, _, data) = await GetByIdAsync(id);
                return (ok, null, data);
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar usuario: {ex.Message}", null);
            }
        }

        // ============================================================
        // ✅ ELIMINAR USUARIO (CAMBIO DE ESTADO A INACTIVO)
        // ============================================================
        public async Task<(bool ok, string? error)> DeleteAsync(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return (false, "Usuario no encontrado");
                }

                usuario.EstadoId = 2;
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar usuario: {ex.Message}");
            }
        }

        // ============================================================
        // 🔧 HELPERS PRIVADOS
        // ============================================================

        private void AgregarTelefonos(int usuarioId, List<string>? telefonos)
        {
            if (telefonos == null) return;

            foreach (var telefono in telefonos)
            {
                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    _context.UsuariosTelefonos.Add(new UsuarioTelefono
                    {
                        UsuarioId = usuarioId,
                        Telefono = telefono
                    });
                }
            }
        }

        private void AgregarRelaciones(int usuarioId, string? institucionId, string? areaId, string? carreraId)
        {
            if (!string.IsNullOrWhiteSpace(institucionId))
            {
                _context.UsuariosInstituciones.Add(new UsuarioInstitucion
                {
                    UsuarioId = usuarioId,
                    InstitucionId = institucionId
                });
            }

            if (!string.IsNullOrWhiteSpace(areaId))
            {
                _context.UsuariosAreas.Add(new UsuarioArea
                {
                    UsuarioId = usuarioId,
                    AreaId = areaId
                });
            }

            if (!string.IsNullOrWhiteSpace(carreraId))
            {
                _context.UsuariosCarreras.Add(new UsuarioCarrera
                {
                    UsuarioId = usuarioId,
                    CarreraId = carreraId
                });
            }
        }

        // Trae los 3 catálogos completos en paralelo (usado para listados)
        private async Task<(Dictionary<int, AreaDto> areas, Dictionary<int, CarreraDto> carreras, Dictionary<int, InstitucionDto> instituciones)> CargarCatalogosAsync()
        {
            var areasTask = _areaClient.GetAllAsync();
            var carrerasTask = _carreraClient.GetAllAsync();
            var institucionesTask = _institucionClient.GetAllAsync();

            await Task.WhenAll(areasTask, carrerasTask, institucionesTask);

            var areasPorId = areasTask.Result.ToDictionary(a => a.Id);
            var carrerasPorId = carrerasTask.Result.ToDictionary(c => c.Id);
            var institucionesPorId = institucionesTask.Result.ToDictionary(i => i.Id);

            return (areasPorId, carrerasPorId, institucionesPorId);
        }

        // Mapeo usando catálogos ya cargados en memoria (para listados masivos)
        private static UsuarioDto MapearUsuarioDto(
            Usuario u,
            Dictionary<int, AreaDto> areasPorId,
            Dictionary<int, CarreraDto> carrerasPorId,
            Dictionary<int, InstitucionDto> institucionesPorId)
        {
            var areas = u.Areas
                .Select(ua => (int.TryParse(ua.AreaId, out var aid) && areasPorId.TryGetValue(aid, out var a))
                    ? a
                    : CrearAreaIndefinida(ua.AreaId))
                .ToList();

            var carreras = u.Carreras
                .Select(uc => (int.TryParse(uc.CarreraId, out var cid) && carrerasPorId.TryGetValue(cid, out var c))
                    ? c
                    : CrearCarreraIndefinida(uc.CarreraId))
                .ToList();

            var instituciones = u.Instituciones
                .Select(ui => (int.TryParse(ui.InstitucionId, out var iid) && institucionesPorId.TryGetValue(iid, out var i))
                    ? i
                    : CrearInstitucionIndefinida(ui.InstitucionId))
                .ToList();

            return ConstruirDto(u, areas, carreras, instituciones);
        }

        // ============================================================
        // 🔧 PLACEHOLDERS "INDEFINIDO"
        // Se usan cuando el usuario SÍ tiene la relación guardada
        // (existe la fila en UsuarioArea/UsuarioCarrera/UsuarioInstitucion)
        // pero el microservicio dueño del catálogo no devolvió el dato
        // (caído, timeout, o el Id ya no existe allá). Así el front
        // siempre recibe algo explícito en vez de que el dato desaparezca
        // silenciosamente de la lista.
        // ============================================================
        private static AreaDto CrearAreaIndefinida(string? rawId) => new()
        {
            Id = int.TryParse(rawId, out var id) ? id : 0,
            Nombre = "Indefinido",
            InstitucionNombre = "Indefinido",
            Activo = false
        };

        private static CarreraDto CrearCarreraIndefinida(string? rawId) => new()
        {
            Id = int.TryParse(rawId, out var id) ? id : 0,
            Nombre = "Indefinido",
            InstitucionNombre = "Indefinido",
            Activo = false
        };

        private static InstitucionDto CrearInstitucionIndefinida(string? rawId) => new()
        {
            Id = int.TryParse(rawId, out var id) ? id : 0,
            Nombre = "Indefinido",
            Activo = false
        };

        // Mapeo consultando directamente los microservicios (usado para un solo usuario)
        private async Task<UsuarioDto> MapearUsuarioDtoConsultandoServiciosAsync(Usuario u)
        {
            var areaTasks = u.Areas
                .Select(ua => int.TryParse(ua.AreaId, out var aid) ? _areaClient.GetByIdAsync(aid) : Task.FromResult<AreaDto?>(null))
                .ToList();

            var carreraTasks = u.Carreras
                .Select(uc => int.TryParse(uc.CarreraId, out var cid) ? _carreraClient.GetByIdAsync(cid) : Task.FromResult<CarreraDto?>(null))
                .ToList();

            var institucionTasks = u.Instituciones
                .Select(ui => int.TryParse(ui.InstitucionId, out var iid) ? _institucionClient.GetByIdAsync(iid) : Task.FromResult<InstitucionDto?>(null))
                .ToList();

            await Task.WhenAll(areaTasks.Cast<Task>()
                .Concat(carreraTasks.Cast<Task>())
                .Concat(institucionTasks.Cast<Task>()));

            var areas = u.Areas
                .Zip(areaTasks, (ua, t) => t.Result ?? CrearAreaIndefinida(ua.AreaId))
                .ToList();

            var carreras = u.Carreras
                .Zip(carreraTasks, (uc, t) => t.Result ?? CrearCarreraIndefinida(uc.CarreraId))
                .ToList();

            var instituciones = u.Instituciones
                .Zip(institucionTasks, (ui, t) => t.Result ?? CrearInstitucionIndefinida(ui.InstitucionId))
                .ToList();

            return ConstruirDto(u, areas, carreras, instituciones);
        }

        private static UsuarioDto ConstruirDto(
            Usuario u,
            List<AreaDto> areas,
            List<CarreraDto> carreras,
            List<InstitucionDto> instituciones)
        {
            return new UsuarioDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                TipoIdentificacion = u.TipoIdentificacion?.Nombre ?? string.Empty,
                NumeroIdentificacion = u.NumeroIdentificacion ?? string.Empty,
                NombreCompleto = u.NombreCompleto ?? string.Empty,
                TipoUsuario = u.TipoUsuario?.Nombre ?? string.Empty,
                Activo = u.EstadoId == 1,
                Bloqueado = u.Bloqueado,
                IntentosFallidos = u.IntentosFallidos,
                FechaCreacion = u.FechaCreacion,
                FotografiaBase64 = u.Fotografia != null ? Convert.ToBase64String(u.Fotografia) : null,
                Telefonos = u.Telefonos?.Select(t => t.Telefono ?? string.Empty).ToList() ?? new List<string>(),
                Confirmado = u.Confirmado,
                RolId = u.RolId,
                EstadoId = u.EstadoId,

                Areas = areas,
                Carreras = carreras,
                Instituciones = instituciones,

                AreaNombre = areas.FirstOrDefault()?.Nombre,
                CarreraNombre = carreras.FirstOrDefault()?.Nombre,
                InstitucionNombre = instituciones.FirstOrDefault()?.Nombre,

                AreaId = u.Areas?.FirstOrDefault()?.AreaId,
                CarreraId = u.Carreras?.FirstOrDefault()?.CarreraId,
                InstitucionId = u.Instituciones?.FirstOrDefault()?.InstitucionId
            };
        }
    }
}