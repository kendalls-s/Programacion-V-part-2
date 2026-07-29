namespace SRV3_Carreras.Auth;

public interface ITokenValidator
{
    Task<bool> ValidateAsync(string token);
}