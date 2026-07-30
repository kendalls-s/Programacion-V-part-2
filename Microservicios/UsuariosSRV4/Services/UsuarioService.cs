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

        public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
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
                    .Where(u => u.EstadoId == 1)
                    .ToListAsync();

                var result = usuarios.Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    TipoIdentificacion = (u.TipoIdentificacion != null) ? u.TipoIdentificacion.Nombre ?? string.Empty : string.Empty,
                    NumeroIdentificacion = u.NumeroIdentificacion ?? string.Empty,
                    NombreCompleto = u.NombreCompleto ?? string.Empty,
                    TipoUsuario = (u.TipoUsuario != null) ? u.TipoUsuario.Nombre ?? string.Empty : string.Empty,
                    Activo = u.EstadoId == 1,
                    Bloqueado = u.Bloqueado,
                    IntentosFallidos = u.IntentosFallidos,
                    FechaCreacion = u.FechaCreacion,
                    FotografiaBase64 = null,
                    Telefonos = (u.Telefonos != null) ? u.Telefonos.Select(t => t.Telefono ?? string.Empty).ToList() : new List<string>(),
                    Confirmado = u.Confirmado
                });

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
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (u == null)
                {
                    return (false, "Usuario no encontrado", null);
                }

                var result = new UsuarioDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    TipoIdentificacion = (u.TipoIdentificacion != null) ? u.TipoIdentificacion.Nombre ?? string.Empty : string.Empty,
                    NumeroIdentificacion = u.NumeroIdentificacion ?? string.Empty,
                    NombreCompleto = u.NombreCompleto ?? string.Empty,
                    TipoUsuario = (u.TipoUsuario != null) ? u.TipoUsuario.Nombre ?? string.Empty : string.Empty,
                    Activo = u.EstadoId == 1,
                    Bloqueado = u.Bloqueado,
                    IntentosFallidos = u.IntentosFallidos,
                    FechaCreacion = u.FechaCreacion,
                    FotografiaBase64 = null,
                    Telefonos = (u.Telefonos != null) ? u.Telefonos.Select(t => t.Telefono ?? string.Empty).ToList() : new List<string>(),
                    Confirmado = u.Confirmado
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
                    RolId = 1,
                    Confirmado = true,
                    FechaCreacion = DateTime.Now,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Fotografia = null
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var telefonosList = dto.Telefonos;
                if (telefonosList != null)
                {
                    foreach (var telefono in telefonosList)
                    {
                        if (!string.IsNullOrWhiteSpace(telefono))
                        {
                            _context.UsuariosTelefonos.Add(new UsuarioTelefono
                            {
                                UsuarioId = usuario.Id,
                                Telefono = telefono ?? string.Empty
                            });
                        }
                    }
                }

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
                usuario.EstadoId = dto.Activo ? 1 : 2;
                usuario.Fotografia = null;
                usuario.Confirmado = dto.Confirmado ?? usuario.Confirmado;

                if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                {
                    usuario.Contrasena = dto.Contrasena ?? string.Empty;
                }

                var telefonosActuales = usuario.Telefonos.ToList();
                foreach (var tel in telefonosActuales)
                {
                    _context.UsuariosTelefonos.Remove(tel);
                }

                var telefonosList = dto.Telefonos;
                if (telefonosList != null)
                {
                    foreach (var telefono in telefonosList)
                    {
                        if (!string.IsNullOrWhiteSpace(telefono))
                        {
                            _context.UsuariosTelefonos.Add(new UsuarioTelefono
                            {
                                UsuarioId = usuario.Id,
                                Telefono = telefono ?? string.Empty
                            });
                        }
                    }
                }

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
    }
}