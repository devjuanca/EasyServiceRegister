using EasyServiceRegister;
using EasyServiceRegister.Sample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(typeof(Program));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/scoped-interface", (IScopedSampleService scopedSampleService) => scopedSampleService.GetId());

app.MapGet("/scoped-service", (ScopedSampleService scopedSampleService) => scopedSampleService.GetId());

app.MapGet("/keyed-scoped-service", ([FromKeyedServices("KeyedScopedSampleServices")] KeyedScopedSampleServices keyedScopedSampleServices) => keyedScopedSampleServices.GetId());

app.MapGet("/singleton-service", (SingletonSampleService singletonSampleService) => singletonSampleService.GetId());

app.MapGet("/transient-service", (TransientSampleService transientSampleService) => transientSampleService.GetId());

app.Run();