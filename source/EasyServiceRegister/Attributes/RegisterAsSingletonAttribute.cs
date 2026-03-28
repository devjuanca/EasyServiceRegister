using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a class to be registered as a singleton service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsSingletonAttribute : Attribute
    {
        /// <summary>
        /// Indicates whether to use TryAddSingleton instead of AddSingleton for registration.
        /// </summary>
        internal bool UseTryAddSingleton { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        /// <summary>
        /// When true, registers the implementation against all its implemented interfaces.
        /// </summary>
        internal bool RegisterAsAllInterfaces { get; set; }

        public RegisterAsSingletonAttribute(Type serviceInterface = null, bool useTryAddSingleton = false, bool registerAsAllInterfaces = false)
        {
            ServiceInterface = serviceInterface;
            UseTryAddSingleton = useTryAddSingleton;
            RegisterAsAllInterfaces = registerAsAllInterfaces;
        }
    }
}
