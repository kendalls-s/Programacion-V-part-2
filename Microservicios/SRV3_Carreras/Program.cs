using SRV3_Carreras;
using SRV3_Carreras.Auth;
using SRV3_Carreras.Repository;
using SRV3_Carreras.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CORS
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// ========================================
// REPOSITORIOS
// ========================================
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();

// ========================================
// SERVICIOS
// ========================================
builder.Services.AddScoped<ICarreraService, CarreraService>();

// ========================================
// TOKEN VALIDATOR
// ========================================
builder.Services.AddHttpClient<ITokenValidator, TokenValidator>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ========================================
// BITÁCORA
// ========================================
builder.Services.AddHttpClient<IBitacoraClient, BitacoraClient>(client =>
{
    string bitacoraUrl = builder.Configuration["Services:Bitacora"]
        ?? throw new InvalidOperationException("No se configuró Services:Bitacora");

    client.BaseAddress = new Uri(bitacoraUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ========================================
// INSTITUCIONES
// ========================================
builder.Services.AddHttpClient<IInstitucionClient, InstitucionClient>(client =>
{
    string institucionesUrl = builder.Configuration["Services:Instituciones"]
        ?? throw new InvalidOperationException("No se configuró Services:Instituciones");

    client.BaseAddress = new Uri(institucionesUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// ========================================
// MIDDLEWARE
// ========================================
app.UseCors("AllowAll");
app.UseRouting();

// ========================================
// ENDPOINTS
// ========================================
app.MapCarreraEndpoints();

app.Run();