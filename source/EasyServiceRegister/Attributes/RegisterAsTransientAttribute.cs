using System;

namespace EasyServiceRegister.Attributes
{

    /// <summary>
    /// Mark a service class to be registered in IoC as Transient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsTransientAttribute : Attribute
    {
        /// <summary>
        /// Defines how to register the service by using TryAddTransient or AddTransient, default value is false.
        /// </summary>
        internal bool UseTryAddTransient { get; set; }
        public RegisterAsTransientAttribute(bool useTryAddTransient = false)
        {
            UseTryAddTransient = useTryAddTransient;
        }
    }
}
