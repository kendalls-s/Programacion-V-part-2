namespace SRV2_Instituciones.Auth
{
    public interface ITokenValidator
    {
        Task<bool> ValidateAsync(string token);
    }
}