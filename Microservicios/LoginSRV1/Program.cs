using LoginSRV1.Data;
using LoginSRV1.Endpoints;
using LoginSRV1.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ AuthDB - para Refresh Tokens (SESION)
// Igual que con la URL de Usuarios: si la config del servidor no trae la cadena
// (appsettings.json desactualizado en el host), usamos un fallback para no romper.
// Lo IDEAL es que el servidor tenga la cadena en su appsettings o en una variable
// de entorno; el fallback es solo una red de seguridad.
var authConnectionString = builder.Configuration.GetConnectionString("AuthDB")
    ?? "Server=tcp:tiusr22pl.cuc-carrera-ti.ac.cr,1433;Database=tiusr22pl_AuthDB;User Id=Admin_Carnet;Password=Admin-12345;TrustServerCertificate=True;MultipleActiveResultSets=True;";

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(authConnectionString));

// ✅ HttpClient + AuthService
// (AddHttpClient<IAuthService, AuthService> ya registra IAuthService,
//  por eso NO se debe volver a registrar con AddScoped: causaba doble registro)
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    var usuariosUrl = builder.Configuration["Services:UsuariosSRV4"]
        ?? "https://tiusr22pl.cuc-carrera-ti.ac.cr/Usuarios";

    // ⚠️ IMPORTANTE: el BaseAddress DEBE terminar en '/'.
    // Si no, .NET descarta el último segmento (/Usuarios) al concatenar la ruta
    // relativa "api/Usuarios/..." y termina llamando a una URL 404, lo que el
    // AuthService interpreta como "Credenciales inválidas".
    if (!usuariosUrl.EndsWith("/"))
        usuariosUrl += "/";

    client.BaseAddress = new Uri(usuariosUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRouting();

app.MapLoginEndpoints();

app.Run();