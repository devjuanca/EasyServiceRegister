## EasyServiceRegister - A easy service registration library

EasyServiceRegister is a library that simplifies the process of registering services with the .NET Core Dependency Injection framework. With version 2.x.x, we have introduced breaking changes, optimized the code, and added new features.
   
More Details below!!

### How to use it:
1. Install the EasyServiceRegister package in your project:
```
dotnet add package EasyServiceRegister
```
2. Add the appropriate class attribute to your service class to indicate how it should be registered with DI:
```
RegisterAsSingleton  --> It will register your service as Singleton.
RegisterAsSingletonKeyed --> It will register your service as Singleton with a key.
RegisterAsScoped    --> It will register your service as Scoped.
RegisterAsScopedKeyed --> It will register your service as Scoped with a key.
RegisterAsTransient --> It will register your service as Transient.
RegisterAsTransientKeyed --> It will register your service as Transient with a key.

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

Example:

Here is an example of a service implementation that registers itself as a scoped service:

```
[RegisterAsScoped]
public class ProductCommandServices : IProductCommandServices
{
    public Task<Product> CreateProduct(Product product)
    {
        // Implementation for creating a product
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
     // Implementation for getting the current user
   }
}
```

With EasyServiceRegister, you can simplify your code by eliminating the need for huge extension methods and a cluttered Startup class.
