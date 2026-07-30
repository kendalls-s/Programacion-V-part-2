namespace TipoIdentificacionSRV6.Auth
{
    public interface ITokenValidator
    {
        Task<bool> ValidateAsync(string token);
    }
}