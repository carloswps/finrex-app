using System.Reflection;
using System.Text;
using Finrex_App.Core.DTOs;
using Finrex_App.Core.Example;
using Finrex_App.Infra.Api.Middleware;
using Finrex_App.Infra.Data;
using Finrex_App.Services;
using Finrex_App.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions( Options =>
    {
        Options.SuppressModelStateInvalidFilter = true;
    } );
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssembly( Assembly.GetExecutingAssembly() );

// DI
builder.Services.AddScoped<IAuthServices, AuthService>();
builder.Services.AddScoped<LoginUserDto, LoginUserDto>();

// Cache config
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

//JWT Config
var jwtKey = builder.Configuration.GetSection( "Jwt:Key" ).Value ??
             throw new InvalidOperationException( "Nenhuma chave encontrada" );
var key = Encoding.UTF8.GetBytes( jwtKey );
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
    .AddJwtBearer( options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey( key ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    } );

// Config Swagger version
builder.Services.AddSwaggerGen( options =>
{
    // Config jwt schema swagger
    options.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    } );

    options.AddSecurityRequirement( new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    } );

    // Documentation XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine( AppContext.BaseDirectory, xmlFile );
    options.IncludeXmlComments( xmlPath );

    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach ( var description in provider.ApiVersionDescriptions )
    {
        options.SwaggerDoc( description.GroupName, new OpenApiInfo
        {
            Title = $"Finrex API {description.ApiVersion}",
            Version = description.ApiVersion.ToString(),
            Description = "API da aplicação Finrex"
        } );
    }

    options.OperationFilter<SwaggerDefaultValues>();

    options.ExampleFilters();
} );
builder.Services.AddSwaggerExamplesFromAssemblyOf<RegisterDtoExample>();

// Setting the API versioning
builder.Services.AddApiVersioning( options =>
{
    options.DefaultApiVersion = new ApiVersion( 1, 0 );
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader( "x-api-version" ),
        new MediaTypeApiVersionReader( "ver" ) );
} );

// Support api version
builder.Services.AddVersionedApiExplorer( options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
} );

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>( options =>
    options.UseNpgsql( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );

// Register services
builder.Services.AddScoped<IAuthServices, AuthService>();

// Add CORS
builder.Services.AddCors( options =>
{
    options.AddPolicy( "AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        } );
} );

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwaggerUI( options =>
    {
        foreach ( var description in provider.ApiVersionDescriptions )
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Finrex API {description.GroupName.ToUpperInvariant()}"
            );
        }

        options.RoutePrefix = "swagger";
    } );
}

//Não usar Https em desenvolvimento
//app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();