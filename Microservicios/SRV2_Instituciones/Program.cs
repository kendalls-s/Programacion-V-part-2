using SRV2_Instituciones;
using SRV2_Instituciones.Auth;
using SRV2_Instituciones.Repository;
using SRV2_Instituciones.Services;

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
builder.Services.AddScoped<
    IDbConnectionFactory,
    DbConnectionFactory>();

builder.Services.AddScoped<
    IInstitucionRepository,
    InstitucionRepository>();

// ========================================
// SERVICIOS
// ========================================
builder.Services.AddScoped<
    IInstitucionService,
    InstitucionService>();

// ========================================
// TOKEN
// ========================================
builder.Services.AddHttpClient<
    ITokenValidator,
    TokenValidator>();

// ========================================
// BITÁCORA
// ========================================
builder.Services.AddHttpClient<
    IBitacoraClient,
    BitacoraClient>();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();

app.MapInstitucionEndpoints();

app.Run();