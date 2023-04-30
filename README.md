## EasyServiceRegister 2.x.x - A easy service registration library

EasyServiceRegister is a library that simplifies the process of registering services with the .NET Core Dependency Injection framework. With version 2.x.x, we have introduced breaking changes, optimized the code, and added new features.
   
More Details below!!

### How to use it:
1. Install the EasyServiceRegister package in your project:
```
dotnet add package EasyServiceRegister --version 2.0.6 (.netstandard 2.0)
```
2. Add the appropriate class attribute to your service class to indicate how it should be registered with DI:
```
RegisterAsSingleton  --> It will register your service as Singleton.
RegisterAsScoped    --> It will register your service as Scoped.
RegisterAsTransient --> It will register your service as Transcient.
```
Note: You can use an optional parameter (useTryAddSingleton,useTryAddScopped or useTryAddTranscient) with the attribute to indicate whether to use TryAdd or Add when registering the service with DI.

3. Register your services by calling the AddServices extension method on the IServiceCollection instance in your Startup.cs or Program.cs file:
```
services.AddServices(params Type[] handlerAssemblyMarkerTypes);

//Example:
services.AddServices(typeof(AssemblyContainingService), typeof(AnotherAssemblyContainingService));
```
This method takes an array of marker types that indicate which assemblies to scan for service implementations.

In case your service class implements an interface to abstract itself, the interface must be the last interface implemented.

##Example

Here is an example of a service implementation that registers itself as a scoped service:

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
And here is an example of a service that registers itself as a singleton service using 'TryAddSingleton'

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

With EasyServiceRegister, you can simplify your code by eliminating the need for huge extension methods and a cluttered Startup class.
