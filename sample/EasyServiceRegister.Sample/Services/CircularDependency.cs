using EasyServiceRegister.Attributes;

//Uncomment the following code to create a circular dependency scenario.

//A → B → C → A

namespace EasyServiceRegister.Sample.Services;

//[RegisterAsScoped]
//public class A(B b)
//{
//}

//[RegisterAsScoped]
//public class B(C c)
//{
//}

//[RegisterAsScoped]
//public class C(A a)
//{
//}