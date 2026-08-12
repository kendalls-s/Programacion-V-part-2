namespace UsuariosSRV4.DTOs
{
    // Refleja la entidad TipoUsuario del microservicio TiposUsuarioSRV5.
    // OJO: a diferencia de Areas/Carreras/Instituciones/TipoIdentificacion,
    // este servicio SI devuelve la lista "pelada" (sin sobre codigo/mensaje/data).
    public class TipoUsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
