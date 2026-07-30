using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ========================================
// URLs de microservicios.
// ========================================
var microBase = (builder.Configuration["MicroservicioBase"] ?? "https://tiusr22pl.cuc-carrera-ti.ac.cr").TrimEnd('/');

string UrlServicio(string clave, string rutaBase)
{
    var config = builder.Configuration[$"Services:{clave}"];
    var url = string.IsNullOrWhiteSpace(config) ? $"{microBase}/{rutaBase}" : config;
    return url.TrimEnd('/') + "/";
}

// HttpClient para LoginSRV1
builder.Services.AddHttpClient("Login", c =>
{
    c.BaseAddress = new Uri(UrlServicio("LoginSRV1", "Login"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para TiposUsuarioSRV5
builder.Services.AddHttpClient("TiposUsuario", c =>
{
    var url = UrlServicio("TiposUsuarioSRV5", "TiposUsuario");
    c.BaseAddress = new Uri(url);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();

// HttpClient para TipoIdentificacionSRV6
builder.Services.AddHttpClient("TipoIdentificacion", c =>
{
    c.BaseAddress = new Uri(UrlServicio("TipoIdentificacionSRV6", "TiposIdentificacion"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ITipoIdentificacionService, TipoIdentificacionService>();

// HttpClient para Fotografia
builder.Services.AddHttpClient("Fotografia", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Fotografia", "Fotografia"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para EstadoUsuario
builder.Services.AddHttpClient("EstadoUsuario", c =>
{
    c.BaseAddress = new Uri(UrlServicio("EstadoUsuario", "EstadoUsuario"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para Parametro
builder.Services.AddHttpClient("Parametro", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Parametro", "Parametro"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para Roles
builder.Services.AddHttpClient("Roles", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Roles", "RolesAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para Áreas
builder.Services.AddHttpClient("Areas", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Areas", "areasAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para Instituciones
builder.Services.AddHttpClient("Instituciones", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Instituciones", "institucionesAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});
//HttpClient para Carreras
builder.Services.AddHttpClient("Carreras", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Carreras", "carrerasAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para Bitácora
builder.Services.AddHttpClient("Bitacora", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Bitacora", "BitacoraAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para CarnetQR
builder.Services.AddHttpClient("CarnetQR", c =>
{
    c.BaseAddress = new Uri(UrlServicio("CarnetQR", "CarnetQR"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para AutoRegistro
builder.Services.AddHttpClient("AutoRegistro", c =>
{
    c.BaseAddress = new Uri(
        UrlServicio("AutoRegistro", "AutoRegistroAPI"));

    c.DefaultRequestHeaders.Add(
        "Accept",
        "application/json");

    c.Timeout = TimeSpan.FromSeconds(30);
});

// Servicios
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IEstadoUsuarioService, EstadoUsuarioService>();
builder.Services.AddScoped<ICarnetQRService, CarnetQRService>();
builder.Services.AddScoped<IFotografiaService, FotografiaService>();
builder.Services.AddScoped<IParametroService, ParametroService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IInstitucionService, InstitucionService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAutoRegistroService,AutoRegistroService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========================================
// Ruta base de la aplicación web.
// Con esto la app se sirve bajo /CarnetWeb
// (ej: https://tiusr22pl.cuc-carrera-ti.ac.cr/CarnetWeb/Login).
// Se puede sobreescribir con la clave "PathBase" en appsettings.
// ========================================
var pathBase = builder.Configuration["PathBase"] ?? "/CarnetWeb";
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// ✅ UN SOLO ENDPOINT /api/login (CON LOGS)
app.MapPost("/api/login", async (LoginRequest request, ILoginService loginService, HttpContext ctx) =>
{
    try
    {
        Console.WriteLine("=== /api/login RECIBIDO ===");
        Console.WriteLine($"Email: {request.Email}");
        Console.WriteLine($"Password: {request.Password}");
        Console.WriteLine($"Tipo: {request.Tipo}");

        if (string.IsNullOrEmpty(request.Email))
        {
            return Results.BadRequest(new { message = "El email es requerido" });
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            return Results.BadRequest(new { message = "La contraseña es requerida" });
        }

        var result = await loginService.LoginAsync(request);

        // Guardar el token en la sesión del servidor
        if (result.Success && !string.IsNullOrEmpty(result.AccessToken))
        {
            ctx.Session.SetString("Token", result.AccessToken);
        }

        // Si no es exitoso, devolver 400 Bad Request con el mensaje
        if (!result.Success)
        {
            return Results.BadRequest(new { message = result.Message ?? "Credenciales inválidas" });
        }

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en /api/login: {ex.Message}");
        return Results.BadRequest(new { message = $"Error: {ex.Message}" });
    }
});

app.MapPost("/api/logout", (HttpContext ctx) =>
{
    ctx.Session.Remove("Token");
    return Results.Ok(new { success = true, message = "Sesión cerrada" });
});

// Endpoint de configuración
app.MapGet("/api/config", (IConfiguration config) =>
{
    var services = new Dictionary<string, string>();
    var servicesSection = config.GetSection("Services");

    foreach (var child in servicesSection.GetChildren())
    {
        services[child.Key] = child.Value ?? string.Empty;
    }

    return Results.Ok(new { Services = services });
});

app.MapRazorPages();

// Endpoint para tipos de usuario
app.MapGet("/api/tipos-usuario", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("TiposUsuario");
        var response = await client.GetAsync("api/TipoUsuario");

        if (!response.IsSuccessStatusCode)
        {
            return Results.BadRequest(new { error = "Error al obtener tipos de usuario" });
        }

        var json = await response.Content.ReadAsStringAsync();
        var tipos = JsonSerializer.Deserialize<object>(json);
        return Results.Ok(tipos);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/", async context =>
{
    var basePath = context.Request.PathBase.Value ?? string.Empty;
    var token = context.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token))
    {
        context.Response.Redirect($"{basePath}/Index");
        return;
    }
    context.Response.Redirect($"{basePath}/Login");
    await Task.CompletedTask;
});

app.Run();