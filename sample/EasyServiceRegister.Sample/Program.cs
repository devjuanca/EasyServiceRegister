using EasyServiceRegister;
using EasyServiceRegister.Sample.Services;
using EasyServiceRegister.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(typeof(Program));

var validationIssues = builder.Services.ValidateServices();

foreach (var issue in validationIssues)
{
    Console.WriteLine(issue);
}


var app = builder.Build();

var registeredServices = ServicesExtension.GetRegisteredServices();

foreach (var service in registeredServices)
{
    Console.WriteLine($"Service Type: {service.ServiceType}, Implementation Type: {service.ImplementationType}, Lifetime: {service.Lifetime}, Key: {service.ServiceKey}, Registration Method: {service.RegistrationMethod}, Attribute Used: {service.AttributeUsed}");
}

app.UseHttpsRedirection();

app.MapGet("/scoped-interface", (IScopedSampleService scopedSampleService) => scopedSampleService.GetId());

app.MapGet("/scoped-service", (ScopedSampleService scopedSampleService) => scopedSampleService.GetId());

app.MapGet("/keyed-scoped-service", ([FromKeyedServices("KeyedScopedSampleServices")] KeyedScopedSampleServices keyedScopedSampleServices) => keyedScopedSampleServices.GetId());

app.MapGet("/singleton-service", (SingletonSampleService singletonSampleService) => singletonSampleService.GetId());

app.MapGet("/transient-service", (TransientSampleService transientSampleService) => transientSampleService.GetId());

app.Run();