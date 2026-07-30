using Dapper;
using SRV11_AutoRegistro.Entities;

namespace SRV11_AutoRegistro.Repository;

public class UsuarioRepository
{
    private readonly IDbConnectionFactory _db;

    public UsuarioRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = _db.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Usuario>(
            """
            SELECT
                Id,
                Email,
                Contrasena,
                TipoUsuarioId,
                EstadoId,
                Confirmado,
                FechaCreacion,
                NombreCompleto,
                TipoIdentificacionId,
                NumeroIdentificacion,
                RolId,
                Fotografia,
                IntentosFallidos,
                Bloqueado,
                FechaBloqueo,
                TokenConfirmacion,
                FechaExpiracion
            FROM dbo.Usuario
            WHERE Email = @email;
            """,
            new { email });
    }

    public async Task<Usuario?> GetByTokenAsync(string token)
    {
        using var connection = _db.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Usuario>(
            """
            SELECT
                Id,
                Email,
                Contrasena,
                TipoUsuarioId,
                EstadoId,
                Confirmado,
                FechaCreacion,
                NombreCompleto,
                TipoIdentificacionId,
                NumeroIdentificacion,
                RolId,
                Fotografia,
                IntentosFallidos,
                Bloqueado,
                FechaBloqueo,
                TokenConfirmacion,
                FechaExpiracion
            FROM dbo.Usuario
            WHERE TokenConfirmacion = @token;
            """,
            new { token });
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.Usuario
            (
                Email,
                Contrasena,
                TipoUsuarioId,
                EstadoId,
                Confirmado,
                FechaCreacion,
                NombreCompleto,
                TipoIdentificacionId,
                NumeroIdentificacion,
                RolId,
                Fotografia,
                IntentosFallidos,
                Bloqueado,
                FechaBloqueo,
                TokenConfirmacion,
                FechaExpiracion
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Email,
                @Contrasena,
                @TipoUsuarioId,
                @EstadoId,
                @Confirmado,
                @FechaCreacion,
                @NombreCompleto,
                @TipoIdentificacionId,
                @NumeroIdentificacion,
                @RolId,
                @Fotografia,
                @IntentosFallidos,
                @Bloqueado,
                @FechaBloqueo,
                @TokenConfirmacion,
                @FechaExpiracion
            );
            """,
            usuario);
    }

    public async Task<int> ConfirmarCuentaAsync(int id)
    {
        using var connection = _db.CreateConnection();

        return await connection.ExecuteAsync(
            """
        UPDATE dbo.Usuario
        SET
            Confirmado = 1,
            TokenConfirmacion = NULL,
            FechaExpiracion = NULL
        WHERE Id = @id
          AND Confirmado = 0;
        """,
            new { id });
    }

    public async Task EliminarUsuarioPendienteAsync(int id)
    {
        using var connection = _db.CreateConnection();

        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "DELETE FROM dbo.UsuarioTelefono WHERE UsuarioId = @id;",
                new { id },
                transaction);

            await connection.ExecuteAsync(
                "DELETE FROM dbo.UsuarioCarrera WHERE UsuarioId = @id;",
                new { id },
                transaction);

            await connection.ExecuteAsync(
                "DELETE FROM dbo.UsuarioArea WHERE UsuarioId = @id;",
                new { id },
                transaction);

            await connection.ExecuteAsync(
                "DELETE FROM dbo.UsuarioInstitucion WHERE UsuarioId = @id;",
                new { id },
                transaction);

            await connection.ExecuteAsync(
                """
                DELETE FROM dbo.Usuario
                WHERE Id = @id
                  AND Confirmado = 0;
                """,
                new { id },
                transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}