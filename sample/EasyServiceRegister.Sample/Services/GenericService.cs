using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

public interface IGenericService<T> where T : class
{
    T DoSomething(T input);
}


[RegisterAsScoped(typeof(IGenericService<>))]
public class GenericService<T> : IGenericService<T> where T : class
{
    public T DoSomething( T input)
    {
        return input;
    }
}
