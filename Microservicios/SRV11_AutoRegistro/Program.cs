using SRV11_AutoRegistro;
using SRV11_AutoRegistro.Repository;
using SRV11_AutoRegistro.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<
    IDbConnectionFactory,
    DbConnectionFactory>();

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<RolRepository>();
builder.Services.AddScoped<UsuarioCarreraRepository>();
builder.Services.AddScoped<UsuarioAreaRepository>();
builder.Services.AddScoped<UsuarioInstitucionRepository>();
builder.Services.AddScoped<UsuarioTelefonoRepository>();

builder.Services.AddScoped<
    IUsuarioService,
    UsuarioService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

builder.Services.AddScoped<BitacoraService>();

builder.Services.AddHttpClient<
    IInstitucionService,
    InstitucionService>();

builder.Services.AddHttpClient<
    ICarreraService,
    CarreraService>();

builder.Services.AddHttpClient<
    IAreaService,
    AreaService>();

builder.Services.AddHttpClient<
    ITipoUsuarioService,
    TipoUsuarioService>();

builder.Services.AddHttpClient<
    ITipoIdentificacionService,
    TipoIdentificacionService>();

builder.Services.AddHttpClient<
    IAuthService,
    AuthService>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapAutoRegistroEndpoints();

app.Run();