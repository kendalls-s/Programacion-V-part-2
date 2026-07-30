using Dapper;

namespace SRV11_AutoRegistro.Repository
{
    public class UsuarioCarreraRepository
    {
        private readonly IDbConnectionFactory _db;

        public UsuarioCarreraRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task AgregarAsync(
            int usuarioId,
            int carreraId)
        {
            using var conn = _db.CreateConnection();

            await conn.ExecuteAsync(
                """
                INSERT INTO dbo.UsuarioCarrera
                (
                    UsuarioId,
                    CarreraId
                )
                VALUES
                (
                    @usuarioId,
                    @carreraId
                );
                """,
                new
                {
                    usuarioId,
                    carreraId = carreraId.ToString()
                });
        }
    }
}