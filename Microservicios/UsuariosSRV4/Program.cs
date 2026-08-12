using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Data;
using UsuariosSRV4.Endpoints;
using UsuariosSRV4.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// ✅ BASE DE DATOS
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// ✅ CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================
// ✅ RAZOR PAGES (panel administrativo en /Pages/Usuarios)
// ============================================================
builder.Services.AddRazorPages();

// ============================================================
// ✅ CONEXIÓN CON LOS MICROSERVICIOS HOSTEADOS (vía Gateway)
// Todos los servicios remotos comparten el mismo dominio base:
//   https://tiusr22pl.cuc-carrera-ti.ac.cr/{NombreDelSitio}
// Cada URL base se define en appsettings.json -> "Services".
// Por ahora solo se configura la conexión (HttpClient + BaseAddress);
// la bitácora se deja pendiente para una siguiente iteración.
// ============================================================

builder.Services.AddHttpClient<IAreaApiClient, AreaApiClient>(client =>
{
    var url = builder.Configuration["Services:AreasSRV4"]
        ?? throw new InvalidOperationException("No se configuró Services:AreasSRV4");

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ICarreraApiClient, CarreraApiClient>(client =>
{
    var url = builder.Configuration["Services:CarrerasSRV3"]
        ?? throw new InvalidOperationException("No se configuró Services:CarrerasSRV3");

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IInstitucionApiClient, InstitucionApiClient>(client =>
{
    var url = builder.Configuration["Services:InstitucionesSRV2"]
        ?? throw new InvalidOperationException("No se configuró Services:InstitucionesSRV2");

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ITipoUsuarioApiClient, TipoUsuarioApiClient>(client =>
{
    var url = builder.Configuration["Services:TiposUsuarioSRV5"]
        ?? throw new InvalidOperationException("No se configuró Services:TiposUsuarioSRV5");

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ITipoIdentificacionApiClient, TipoIdentificacionApiClient>(client =>
{
    var url = builder.Configuration["Services:TipoIdentificacionSRV6"]
        ?? throw new InvalidOperationException("No se configuró Services:TipoIdentificacionSRV6");

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ============================================================
// ✅ SERVICIOS DE NEGOCIO
// ============================================================
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();

// ============================================================
// ✅ ENDPOINTS DE USUARIOS (login, CRUD, refresh-token, etc.)
// ============================================================
app.MapUsuarioEndpoints();

app.MapRazorPages();

app.Run();
