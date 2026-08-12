namespace UsuariosSRV4.DTOs
{
    // Refleja la entidad Institucion del microservicio SRV2_Instituciones
    public class InstitucionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Dominios { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    // SRV2_Instituciones responde SIEMPRE envuelto: { codigo, mensaje, data }
    public class InstitucionEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public InstitucionDto? Data { get; set; }
    }

    public class InstitucionListEnvelope
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public List<InstitucionDto> Data { get; set; } = new();
    }
}
