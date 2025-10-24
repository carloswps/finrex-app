using System.Reflection;
using Finrex_App.Application.Validators;
using Finrex_App.Infra.Api.Middleware;
using Finrex_App.Infra.Data;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Scalar.AspNetCore;
using Finrex_App.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.Services
    .AddApiConfiguration( builder.Configuration )
    .AddSwaggerConfiguration( builder )
    .AddAuthenticationConfiguration( builder.Configuration )
    .AddCorsConfiguration()
    .AddApplicationServices();

var app = builder.Build();

app.ConfigureMiddleware( app.Environment );
app.Run();