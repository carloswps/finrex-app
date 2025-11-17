using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Finrex_App.Application.JwtGenerate;
using Finrex_App.Application.Services;
using Finrex_App.Application.Services.Interface;
using Finrex_App.Infra.Api.Middleware;
using Finrex_App.Infra.Data;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Finrex_App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration )
    {
        services.AddControllers()
            .AddJsonOptions( options =>
            {
                options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
            } )
            .ConfigureApiBehaviorOptions( options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            } );

        services.AddAntiforgery( options => options.HeaderName = "X-CSRF-TOKEN" );
        services.AddLogging();
        services.AddMemoryCache();
        services.AddResponseCaching();

        services.AddDbContext<AppDbContext>( options =>
            options.UseNpgsql( configuration.GetConnectionString( "DefaultConnection" ) ) );

        services.AddApiVersioning( options =>
        {
            options.DefaultApiVersion = new ApiVersion( 1, 0 );
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true; // Adicionado para consistência
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader( "x-api-version" ),
                new MediaTypeApiVersionReader( "ver" )
            );
        } );

        services.AddVersionedApiExplorer( options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        } );
        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services, WebApplicationBuilder builder )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen( options =>
        {
            options.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme
            {
                Description
                    = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                            Type = ReferenceType.SecurityScheme, Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            } );

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine( AppContext.BaseDirectory, xmlFile );
            options.IncludeXmlComments( xmlPath, true );

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
                        Name = "Finrex.APP", Email = "seu-email@example.com",
                        Url = new Uri( "https://seusite.com" )
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Licença MIT", Url = new Uri( "https://opensource.org/licenses/MIT" )
                    }
                } );
            }

            options.AddServer( new OpenApiServer { Url = "http://localhost:5023" } );
        } );
        return services;
    }

    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services, IConfiguration configuration )
    {
        services.AddAuthentication( options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            } )
            .AddJwtBearer( options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                var keyString = configuration[ "Jwt:Key" ];
                if ( string.IsNullOrWhiteSpace( keyString ) )
                {
                    // Fallback development key to avoid null reference on environments without user-secrets
                    keyString = "finrex-dev-secret-key-change-in-production-0123456789ABCDEF";
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( keyString ) ),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration[ "Jwt:Issuer" ],
                    ValidAudience = configuration[ "Jwt:Audience" ],
                    ClockSkew = TimeSpan.FromMinutes( 5 )
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var hasBearer = context.Request.Headers[ "Authorization" ].FirstOrDefault()
                            ?.StartsWith( "Bearer " ) == true;
                        if ( !hasBearer && context.Request.Cookies.ContainsKey( "finrex.auth" ) )
                        {
                            context.Token = context.Request.Cookies[ "finrex.auth" ];
                        }

                        return Task.CompletedTask;
                    }
                };
            } )
            .AddCookie( "Cookies" )
            .AddGoogle( options =>
            {
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.SignInScheme = "Cookies";
                options.ClientId = configuration[ "Authentication:Google:ClientId" ];
                options.ClientSecret = configuration[ "Authentication:Google:ClientSecret" ];
                options.CallbackPath = "/api/v1.0/login-users/google-signin-callback";
            } );

        return services;
    }

    public static IServiceCollection AddCorsConfiguration( this IServiceCollection services )
    {
        services.AddCors( options =>
        {
            options.AddPolicy( "AllowAll", corsPolicyBuilder =>
            {
                corsPolicyBuilder.WithOrigins( "http://localhost:3000" )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            } );
        } );

        return services;
    }

    public static IServiceCollection AddApplicationServices( this IServiceCollection services )
    {
        services.AddValidatorsFromAssembly( Assembly.GetExecutingAssembly() );
        services.AddMapster();

        services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
        services.AddScoped<ILoginUserServices, LoginUserService>();
        services.AddScoped<TokeService>();
        services.AddScoped<IFinanceFactorsService, FinanceFactorsService>();

        return services;
    }

    public static WebApplication ConfigureMiddleware( this WebApplication app, IWebHostEnvironment env )
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if ( env.IsDevelopment() )
        {
            app.UseSwagger();
            var apiVersionDescriptionProvider =
                app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwaggerUI( options =>
            {
                foreach ( var description in apiVersionDescriptionProvider.ApiVersionDescriptions )
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"Finrex API {description.GroupName.ToUpperInvariant()}" );
                }

                options.RoutePrefix = "swagger";
            } );
        }

        app.UseCors( "AllowAll" );
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        //app.MapScalarApiReference(); 

        app.MapGet( "/docs", context =>
        {
            context.Response.Redirect( "https://finance-api.apidocumentation.com/finrex-api-10" );
            return Task.CompletedTask;
        } );

        return app;
    }
}