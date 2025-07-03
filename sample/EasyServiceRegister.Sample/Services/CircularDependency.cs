using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Sample.Services;

[RegisterAsScoped]
public class A
{
    public A(B b)
    {

    }
}

[RegisterAsScoped]
public class B
{
    public B(C c)
    {
    }
}

[RegisterAsScoped]
public class C
{
    public C(A a)
    {
    }
}
// A → B → C → A