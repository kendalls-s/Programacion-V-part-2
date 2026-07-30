using Dapper;

namespace SRV11_AutoRegistro.Repository
{
    public class UsuarioInstitucionRepository
    {
        private readonly IDbConnectionFactory _db;

        public UsuarioInstitucionRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task AgregarAsync(int usuarioId, int institucionId)
        {
            using var conn = _db.CreateConnection();

            await conn.ExecuteAsync(
                """
                INSERT INTO dbo.UsuarioInstitucion
                (
                    UsuarioId,
                    InstitucionId
                )
                VALUES
                (
                    @usuarioId,
                    @institucionId
                );
                """,
                new
                {
                    usuarioId,
                    institucionId = institucionId.ToString()
                });
        }
    }
}