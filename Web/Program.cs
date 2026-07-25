using CarnetDigitalWeb.Models;
using CarnetDigitalWeb.Services;

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
// Todos cuelgan de MicroservicioBase con un prefijo de ruta por servicio.
// La barra final es OBLIGATORIA: sin ella, al combinar con rutas relativas
// (p.ej. "api/auth/login") .NET descarta el prefijo /Login y la llamada falla.
// Cada uno se puede sobreescribir con Services:<clave> en appsettings.json.
// ========================================
var microBase = (builder.Configuration["MicroservicioBase"] ?? "https://tiusr22pl.cuc-carrera-ti.ac.cr").TrimEnd('/');

// Devuelve la URL configurada en Services:<clave> (con barra final) o {MicroservicioBase}/<rutaBase>/
string UrlServicio(string clave, string rutaBase)
{
    var config = builder.Configuration[$"Services:{clave}"];
    var url = string.IsNullOrWhiteSpace(config) ? $"{microBase}/{rutaBase}" : config;
    return url.TrimEnd('/') + "/";
}

// HttpClient para LoginSRV1  (/Login)
builder.Services.AddHttpClient("Login", c =>
{
    c.BaseAddress = new Uri(UrlServicio("LoginSRV1", "Login"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para TiposUsuarioSRV5  (/TiposUsuario)
builder.Services.AddHttpClient("TipoUsuario", c =>
{
    c.BaseAddress = new Uri(UrlServicio("TiposUsuarioSRV5", "TiposUsuario"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();

// HttpClient para TipoIdentificacionSRV6  (/TiposIdentificacion)
builder.Services.AddHttpClient("TipoIdentificacion", c =>
{
    c.BaseAddress = new Uri(UrlServicio("TipoIdentificacionSRV6", "TiposIdentificacion"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ITipoIdentificacionService, TipoIdentificacionService>();

// ✅ HttpClient para SRV13 - Fotografia
builder.Services.AddHttpClient("Fotografia", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Fotografia", "Fotografia"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// ✅ HttpClient para SRV12 - EstadoUsuario
builder.Services.AddHttpClient("EstadoUsuario", c =>
{
    c.BaseAddress = new Uri(UrlServicio("EstadoUsuario", "EstadoUsuario"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// ✅ HttpClient para SRV15 - Parametro
builder.Services.AddHttpClient("Parametro", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Parametro", "Parametro"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// ✅ HttpClient para SRV14 - CarnetQR
builder.Services.AddHttpClient("CarnetQR", c =>
{
    c.BaseAddress = new Uri(UrlServicio("CarnetQR", "CarnetQR"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// ✅ Servicios
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IEstadoUsuarioService, EstadoUsuarioService>();
builder.Services.AddScoped<ICarnetQRService, CarnetQRService>();
builder.Services.AddScoped<IFotografiaService, FotografiaService>();
builder.Services.AddScoped<IParametroService, ParametroService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapGet("/set-token", (HttpContext ctx) =>
{
    var token = ctx.Request.Query["token"].ToString();
    var returnUrl = ctx.Request.Query["returnUrl"].ToString();

    if (!string.IsNullOrEmpty(token))
    {
        ctx.Session.SetString("Token", token);
    }

    var redirectUrl = string.IsNullOrEmpty(returnUrl) ? "/EstadoUsuario" : returnUrl;
    return Results.Redirect(redirectUrl);
});

app.MapPost("/api/login", async (LoginRequest request, ILoginService loginService, HttpContext ctx) =>
{
    var result = await loginService.LoginAsync(request);

    // ✅ Guardar el token en la sesión del servidor para las páginas server-side
    // (Fotografía, Cambio de Estado, Parámetros, QR leen HttpContext.Session["Token"])
    if (result.Success && !string.IsNullOrEmpty(result.AccessToken))
    {
        ctx.Session.SetString("Token", result.AccessToken);
    }

    return Results.Ok(result);
});

app.MapPost("/api/logout", (HttpContext ctx) =>
{
    ctx.Session.Remove("Token");
    return Results.Ok(new { success = true, message = "Sesión cerrada" });
});

// ✅ Endpoint de configuración para el frontend
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

app.MapGet("/", async context =>
{
    var token = context.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token))
    {
        context.Response.Redirect("/EstadoUsuario");
        return;
    }
    context.Response.Redirect("/Login");
    await Task.CompletedTask;
});

app.Run();