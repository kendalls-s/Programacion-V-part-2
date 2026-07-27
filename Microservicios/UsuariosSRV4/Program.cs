using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Data;
using UsuariosSRV4.Endpoints;
using UsuariosSRV4.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// CORS
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

// ✅ IMPORTANTE: Registrar los endpoints
app.MapUsuarioEndpoints();

app.Run();