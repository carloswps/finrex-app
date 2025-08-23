using System.Reflection;
using System.Text;
using Finrex_App.Application.DTOs;
using Finrex_App.Application.JwtGenerate;
using Finrex_App.Application.Services;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Api.Example;
using Finrex_App.Infra.Api.Filters;
using Finrex_App.Infra.Api.Middleware;
using Finrex_App.Infra.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using FluentValidation;
using Mapster;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder( args );

// Sentry config 
builder.WebHost.UseSentry( o =>
{
    o.Dsn = "https://53862f71d78a57b247feb41bb7aaf48c@o4509851494842368.ingest.us.sentry.io/4509851504607232";
    o.Debug = true;
    o.SendClientReports = true;
    o.TracesSampleRate = 1.0;
    o.ProfilesSampleRate = 1.0;
} );


// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions( Options =>
    {
        Options.SuppressModelStateInvalidFilter = true;
    } );
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssembly( Assembly.GetExecutingAssembly() );
builder.Services.AddLogging();
builder.Services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
builder.Services.AddScoped<ILoginUserServices, LoginUserService>();
builder.Services.AddScoped<TokeService>();
builder.Services.AddScoped<LoginUserDto, LoginUserDto>();
builder.Services.AddScoped<TokeService>();
builder.Services.AddScoped<MIncomeDTOValidator>();
builder.Services.AddScoped<MSpendingDTOValidator>();
builder.Services.AddMapster();

// Cache config
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Add config CORS
builder.Services.AddCors( options =>
{
    options.AddPolicy( "AllowAll",
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        } );
} );

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes( builder.Configuration[ "Jwt:Key" ] ) ),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration[ "Jwt:Issuer" ],
            ValidAudience = builder.Configuration[ "Jwt:Audience" ]
        };
    } );

// Config Swagger version and Scalar documentation
builder.Services.AddEndpointsApiExplorer();

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
            Description
                = "Uma API para gerenciamento de finanças pessoais, permitindo o registro de receitas e despesas.",
            Contact = new OpenApiContact
            {
                Name = "Seu Nome",
                Email = "seu-email@example.com",
                Url = new Uri( "https://seusite.com" )
            },
            License = new OpenApiLicense
            {
                Name = "Licença MIT",
                Url = new Uri( "https://opensource.org/licenses/MIT" )
            }
        } );

        options.AddServer( new OpenApiServer { Url = "http://localhost:5023" } );
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();

    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI( options =>
    {
        foreach ( var description in apiVersionDescriptionProvider.ApiVersionDescriptions )
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Finrex API {description.GroupName.ToUpperInvariant()}"
            );
        }

        options.RoutePrefix = "swagger";
    } );
} else
{
    app.UseSwaggerUI( options =>
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach ( var description in apiVersionDescriptionProvider.ApiVersionDescriptions )
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
app.UseCors( "AllowAll" );
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapScalarApiReference();
app.MapControllers();

app.MapGet( "/docs", context =>
{
    context.Response.Redirect( "https://finance-api.apidocumentation.com/finrex-api-10" );
    return Task.CompletedTask;
} );

app.Run();