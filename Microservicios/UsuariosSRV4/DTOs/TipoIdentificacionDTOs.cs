namespace UsuariosSRV4.DTOs
{
    // Refleja la entidad TipoIdentificacion del microservicio TipoIdentificacionSRV6
    public class TipoIdentificacionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    // TipoIdentificacionSRV6 responde envuelto: { codigo, mensaje, data }
    public class TipoIdentificacionEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public TipoIdentificacionDto? Data { get; set; }
    }

    public class TipoIdentificacionListEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public List<TipoIdentificacionDto> Data { get; set; } = new();
    }
}
