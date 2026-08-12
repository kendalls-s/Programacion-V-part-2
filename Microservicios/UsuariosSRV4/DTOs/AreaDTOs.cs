namespace UsuariosSRV4.DTOs
{
    // Refleja la entidad AreaTrabajo del microservicio SRV4_Areas
    public class AreaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int InstitucionId { get; set; }
        public string InstitucionNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    // SRV4_Areas responde SIEMPRE envuelto: { codigo, mensaje, data }
    public class AreaEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public AreaDto? Data { get; set; }
    }

    public class AreaListEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public List<AreaDto> Data { get; set; } = new();
    }
}
