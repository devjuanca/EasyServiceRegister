using Infrastructure;
using Presentation;
using Presentation.EndpointsRegistration;
using EasyServiceRegister;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.AddServices();

// Registering in DI container all services found in Presentation and Infrastructure assemblies.

builder.Services.AddServices(typeof(DependencyInjection), typeof(Program));

builder.Services.AddEndpointDefinitions(configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.UseEndpointDefinitions();

await app.ApplySeeder();

await app.RunAsync();

