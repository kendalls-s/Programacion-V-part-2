namespace CarnetDigitalWeb.Models
{
    public class BitacoraRegistroModel
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Accion { get; set; } = string.Empty;

        public string? DetalleJson { get; set; }

        public bool EsError { get; set; }
    }

    public class BitacoraRespuestaModel
    {
        public List<BitacoraRegistroModel> Registros { get; set; } = [];

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }

        public int PaginaActual { get; set; }

        public int TamanoPagina { get; set; }
    }
}