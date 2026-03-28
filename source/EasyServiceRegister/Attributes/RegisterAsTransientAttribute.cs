using System;

namespace EasyServiceRegister.Attributes
{

    /// <summary>
    /// Marks a class to be registered as a transient service with a specific key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsTransientAttribute : Attribute
    {
        /// <summary>
        /// Defines how to register the service by using TryAddTransient or AddTransient, default value is false.
        /// </summary>
        internal bool UseTryAddTransient { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        /// <summary>
        /// When true, registers the implementation against all its implemented interfaces.
        /// </summary>
        internal bool RegisterAsAllInterfaces { get; set; }

        public RegisterAsTransientAttribute(Type serviceInterface = null, bool useTryAddTransient = false, bool registerAsAllInterfaces = false)
        {
            ServiceInterface = serviceInterface;
            UseTryAddTransient = useTryAddTransient;
            RegisterAsAllInterfaces = registerAsAllInterfaces;
        }
    }
}
