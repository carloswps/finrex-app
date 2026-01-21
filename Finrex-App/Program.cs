using Finrex_App.Infra.Data;
using Finrex_App.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiConfiguration(builder.Configuration)
    .AddSwaggerConfiguration(builder)
    .AddAuthenticationConfiguration(builder.Configuration)
    .AddCorsConfiguration()
    .AddApplicationServices();

var app = builder.Build();


app.ConfigureMiddleware(app.Environment);
app.Run();