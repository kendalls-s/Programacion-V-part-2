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

// HttpClient para SRV8 - Roles
builder.Services.AddHttpClient("Roles", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Roles", "RolesAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para SRV4 - Áreas
builder.Services.AddHttpClient("Areas", c =>
{
    c.BaseAddress = new Uri(UrlServicio("Areas", "areasAPI"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para SRV2 - Instituciones
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

// HttpClient para SRV9 - Bitácora
builder.Services.AddHttpClient("Bitacora", c =>
{
    c.BaseAddress = new Uri(
        UrlServicio("Bitacora", "BitacoraAPI")
    );

    c.DefaultRequestHeaders.Add(
        "Accept",
        "application/json"
    );

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
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IInstitucionService, InstitucionService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
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
    try
    {
        // ✅ LOG PARA DEPURACIÓN
        Console.WriteLine("=== /api/login RECIBIDO ===");
        Console.WriteLine($"Email: {request.Email}");
        Console.WriteLine($"Password: {request.Password}");
        Console.WriteLine($"Tipo: {request.Tipo}");

        // ✅ VALIDAR QUE LOS DATOS NO ESTÉN VACÍOS
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