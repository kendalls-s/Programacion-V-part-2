namespace SRV4_Areas.Auth;

public interface ITokenValidator
{
    Task<bool> ValidateAsync(string token);
}