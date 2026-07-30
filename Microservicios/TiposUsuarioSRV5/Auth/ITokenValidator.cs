namespace TiposUsuarioSRV5.Auth
{
    public interface ITokenValidator
    {
        Task<bool> ValidateAsync(string token);
    }
}