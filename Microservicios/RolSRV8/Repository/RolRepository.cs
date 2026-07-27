using Dapper;
using RolSRV8.Entities;

namespace RolSRV8.Repository;

public class RolRepository
{
    private readonly IDbConnectionFactory _factory;


    public RolRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }



    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
    {
        using var connection = _factory.CreateConnection();


        var sql = @"
            SELECT 
                r.Id,
                r.Nombre
            FROM Rol r
            ORDER BY r.Id;
        ";


        var roles = await connection.QueryAsync<Rol>(sql);



        foreach (var rol in roles)
        {
            rol.Pantallas =
                (await ObtenerPantallasRolAsync(rol.Id))
                .ToList();
        }


        return roles;
    }





    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();


        var sql = @"
            SELECT 
                Id,
                Nombre
            FROM Rol
            WHERE Id = @Id;
        ";


        var rol = await connection.QueryFirstOrDefaultAsync<Rol>(
            sql,
            new { Id = id });



        if (rol != null)
        {
            rol.Pantallas =
                (await ObtenerPantallasRolAsync(id))
                .ToList();
        }


        return rol;
    }





    private async Task<IEnumerable<Pantalla>> ObtenerPantallasRolAsync(int rolId)
    {
        using var connection = _factory.CreateConnection();


        var sql = @"
            SELECT
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Ruta
            FROM Pantalla p
            INNER JOIN RolPantalla rp
                ON rp.PantallaId = p.Id
            WHERE rp.RolId = @RolId
            ORDER BY p.Id;
        ";


        return await connection.QueryAsync<Pantalla>(
            sql,
            new { RolId = rolId });
    }





    public async Task<int> CrearAsync(
        Rol rol,
        List<int> pantallas)
    {

        using var connection = _factory.CreateConnection();

        connection.Open();


        using var transaction =
            connection.BeginTransaction();



        try
        {

            var sqlRol = @"
                INSERT INTO Rol(Nombre)
                VALUES(@Nombre);

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";



            var id = await connection.ExecuteScalarAsync<int>(
                sqlRol,
                rol,
                transaction);



            foreach (var pantallaId in pantallas)
            {

                await connection.ExecuteAsync(
                    @"
                    INSERT INTO RolPantalla
                    (
                        RolId,
                        PantallaId
                    )
                    VALUES
                    (
                        @RolId,
                        @PantallaId
                    );
                    ",
                    new
                    {
                        RolId = id,
                        PantallaId = pantallaId
                    },
                    transaction);
            }



            transaction.Commit();


            return id;

        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }






    public async Task ActualizarAsync(
        int id,
        string nombre,
        List<int> pantallas)
    {

        using var connection = _factory.CreateConnection();


        connection.Open();


        using var transaction =
            connection.BeginTransaction();



        try
        {

            await connection.ExecuteAsync(
                @"
                UPDATE Rol
                SET Nombre = @Nombre
                WHERE Id = @Id;
                ",
                new
                {
                    Id = id,
                    Nombre = nombre
                },
                transaction);




            await connection.ExecuteAsync(
                @"
                DELETE FROM RolPantalla
                WHERE RolId = @Id;
                ",
                new
                {
                    Id = id
                },
                transaction);




            foreach (var pantallaId in pantallas)
            {

                await connection.ExecuteAsync(
                    @"
                    INSERT INTO RolPantalla
                    (
                        RolId,
                        PantallaId
                    )
                    VALUES
                    (
                        @RolId,
                        @PantallaId
                    );
                    ",
                    new
                    {
                        RolId = id,
                        PantallaId = pantallaId
                    },
                    transaction);
            }



            transaction.Commit();

        }
        catch
        {
            transaction.Rollback();
            throw;
        }

    }






    public async Task EliminarAsync(int id)
    {

        using var connection = _factory.CreateConnection();


        await connection.ExecuteAsync(
            @"
            DELETE FROM RolPantalla
            WHERE RolId = @Id;


            DELETE FROM Rol
            WHERE Id = @Id;
            ",
            new
            {
                Id = id
            });
    }


    public async Task<int> ContarUsuariosAsync(int id)
    {

        using var connection = _factory.CreateConnection();



        return await connection.ExecuteScalarAsync<int>(
            @"
            SELECT COUNT(*)
            FROM Usuario
            WHERE RolId = @Id;
            ",
            new
            {
                Id = id
            });
    }

}