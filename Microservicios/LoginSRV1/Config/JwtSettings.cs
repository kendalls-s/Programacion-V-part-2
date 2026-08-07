namespace LoginSRV1.Config
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 5;
        public int RefreshTokenExpirationMinutes { get; set; } = 60;
    }
}