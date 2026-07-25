using System.Text.Json.Serialization;

namespace LoginSRV1.DTOs
{
    // ---------- Request (headers) ----------
    // Los datos de /login llegan por headers, no por body.
    // Esta clase se usa solo internamente para agrupar los valores ya leídos.
    public class LoginRequestDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    // ---------- Respuesta exitosa /login (201) ----------
    public class LoginSuccessResponseDto
    {
        [JsonPropertyName("expires_in")]
        public DateTimeOffset ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("usuarioID")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("institutions")]
        public List<InstitutionDto> Institutions { get; set; } = new();

        // --- Otra info ---
        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("tipoUsuario")]
        public string TipoUsuario { get; set; } = string.Empty;
    }

    public class InstitutionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    // ---------- Respuesta exitosa /refresh (201) ----------
    public class RefreshResponseDto
    {
        [JsonPropertyName("expires_in")]
        public DateTimeOffset ExpiresIn { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // ---------- Respuesta de error (400 / 401) ----------
    public class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }

    // ---------- Info de usuario obtenida de UsuariosSRV4 ----------
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int? TipoUsuarioId { get; set; }
        public int? RolId { get; set; }
        public string? Rol { get; set; }
        public List<InstitutionDto>? Institutions { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ValidarCredencialesResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
        public int IntentosFallidos { get; set; }
        public int TipoUsuarioId { get; set; }
        public int RolId { get; set; }
        public List<InstitutionDto>? Institutions { get; set; }
    }

    // ---------- Resultado interno de las operaciones del servicio ----------
    public enum AuthErrorType
    {
        None,
        Validation,   // datos faltantes / vacíos -> 400
        Unauthorized  // credenciales / token inválido -> 401
    }

    public class AuthOperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public AuthErrorType ErrorType { get; set; } = AuthErrorType.None;

        public static AuthOperationResult<T> Ok(T data) =>
            new() { Success = true, Data = data, ErrorType = AuthErrorType.None };

        public static AuthOperationResult<T> Fail(string message, AuthErrorType errorType) =>
            new() { Success = false, ErrorMessage = message, ErrorType = errorType };
    }
}
