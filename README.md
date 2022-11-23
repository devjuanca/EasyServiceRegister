## Version 2.0.0 of EasyServiceRegister has some break changes, code has been optimized and other cool stuff has been added!!

### How to use it:
1. First you will need to install the package in the project where your services implementations will be.
```
dotnet add package EasyServiceRegister --version 2.0.3 (.net 6 or supperior)
```
2. Then in each service class you must add one of the following class attributes:
```
RegisterAsSingleton  --> It will register your service as Singleton.
RegisterAsScoped    --> It will register your service as Scoped.
RegisterAsTransient --> It will register your service as Transcient.
```
note: Each attribute has a parameter indicating if your service must be registered using TryAdd or just Add, by default this property will be false. 

3. Finally you must register your services using the following extension method
```
services.AddServices(params Type[] handlerAssemblyMarkerTypes);
```
Each handlerAssemblyMarkerTypes must be a type from the assembly where your services are.

Here is an example of a service implementation:
```
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// In this case services from two differents assemblies are been registered into IoC 
builder.Services.AddServices(typeof(DependencyInjection), typeof(Program));

builder.Services.AddEndpointDefinitions(configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.UseEndpointDefinitions();

await app.ApplySeeder();

await app.RunAsync();
```



```
[RegisterAsScoped]
public class ProductCommandServices : IProductCommandServices
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductCommandServices(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
}
```
```
[RegisterAsSingleton(useTryAddSingleton: true)]
public class GetCurrentUser
{
  public Task<User> GetCurrentUser()
   {
     ...
   }
}
```

Consider the following:
In case your service implement an interface to abstract itself, that interface must be the last you implement.
  Ex.
```
[RegisterAsScoped]
 public class ProductCommandServices : ISomeOtherInterface, IProductCommandServices
{
 ...
}
```
And that's it!! No more loaded Startup classes or huge extension methods registering services in your code. :)
