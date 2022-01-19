using Presentation;
using Presentation.EndpointsRegistration;
using ServiceInyector;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.AddServices();

builder.Services.AddServices(new List<string> { "Infrastructure", "Presentation" });

builder.Services.AddEndpointDefinitions(configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
app.ConfigurePipeline();

app.UseEndpointDefinitions();

await app.ApplySeeder();

await app.RunAsync();

