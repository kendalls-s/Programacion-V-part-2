using Microsoft.EntityFrameworkCore;
using TipoIdentificacionSRV6.Data;
using TipoIdentificacionSRV6.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient para validar el token contra LoginSRV1 (endpoint /api/auth/validate)
builder.Services.AddHttpClient("LoginApi", client =>
{
    var url = builder.Configuration["Services:LoginApi"] ?? "https://localhost:7019";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(10);
});

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

app.MapTipoIdentificacionEndpoints();

app.Run();
