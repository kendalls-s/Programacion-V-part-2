using RolSRV8;
using RolSRV8.Auth;
using RolSRV8.Repository;
using RolSRV8.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<RolRepository>();

builder.Services.AddScoped<IRolService, RolService>();


// Bitacora
builder.Services.AddHttpClient<IBitacoraClient, BitacoraClient>();


// Token
builder.Services.AddHttpClient<ITokenValidator, TokenValidator>();


var app = builder.Build();


app.UseRouting();


app.MapRolEndpoints();


app.Run();