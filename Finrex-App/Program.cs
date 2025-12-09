using Finrex_App.Infra.Data;
using Finrex_App.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder( args );

builder.Services
    .AddApiConfiguration( builder.Configuration )
    .AddSwaggerConfiguration( builder )
    .AddAuthenticationConfiguration( builder.Configuration )
    .AddCorsConfiguration()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.ConfigureMiddleware( app.Environment );
app.Run();