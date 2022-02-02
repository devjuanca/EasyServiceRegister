## This is a package that makes it easier to register services in your .Net application.

### How to use it:
1. First you will need to install the package in the project where your services implementations will be.
```
Install-Package EasyServiceRegister -Version 0.0.8  (for .Net Core 3.1 projects)
```
```
Install-Package EasyServiceRegister -Version 0.0.9  (for .Net 5 projects)
```
```
Install-Package EasyServiceRegister -Version 1.0.0  (for .Net 6 projects)
```
2. Then in each service class you must implement one of the following interfaces:
```
IRegisterAsSingleton  --> It will register your service as Singleton.
IRegisterAsScoped     --> It will register your service as Scoped.
IRegisterAsTranscient --> It will register your service as Transcient.
```
3. Finally in your Startup class or Program you must add the next extension method:
```
services.AddServices(new List<string> { "Infrastructure" });
```
The method receives a list of project names where your services implementations are.

Here is an example of a service implementation:
```
public class ProductCommandServices : IProductCommandServices, IRegisterAsScoped
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
Consider the following:
```diff
* First interface must always be the service abstraction, second one must be the corresponding Easy Service Register interface. 
  (Check the example)

* The abstractions must be in a different project than the implementations. (something like, Application and Infrastructure)
```

And that's it!! No more loaded Startup classes or huge extension methods registering services in your code. :)
