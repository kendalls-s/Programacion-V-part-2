namespace CarnetDigitalWeb.Models
{
    public class RolApiResponse<T>
    {
        public int Codigo { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public T? Data { get; set; }
    }
}