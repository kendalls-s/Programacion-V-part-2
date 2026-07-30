using Dapper;
using SRV11_AutoRegistro.Entities;

namespace SRV11_AutoRegistro.Repository;

public class RolRepository
{
    private readonly IDbConnectionFactory _db;

    public RolRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
    {
        using var connection = _db.CreateConnection();

        return await connection.QueryAsync<Rol>(
            """
            SELECT
                Id,
                Nombre
            FROM dbo.Rol
            ORDER BY Nombre;
            """);
    }

    public async Task<bool> ExisteAsync(int id)
    {
        using var connection = _db.CreateConnection();

        var cantidad = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1)
            FROM dbo.Rol
            WHERE Id = @id;
            """,
            new { id });

        return cantidad > 0;
    }
}