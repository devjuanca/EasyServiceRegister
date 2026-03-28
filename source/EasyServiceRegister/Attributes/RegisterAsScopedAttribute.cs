using System;

namespace EasyServiceRegister.Attributes

{
    /// <summary>
    /// Marks a class to be registered as a scoped service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsScopedAttribute : Attribute
    {
        /// <summary>
        /// Indicates whether to use TryAddScoped instead of AddScoped for registration.
        /// </summary>
        internal bool UseTryAddScoped { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        /// <summary>
        /// When true, registers the implementation against all its implemented interfaces.
        /// </summary>
        internal bool RegisterAsAllInterfaces { get; set; }

        public RegisterAsScopedAttribute(Type serviceInterface = null, bool useTryAddScoped = false, bool registerAsAllInterfaces = false)
        {
            ServiceInterface = serviceInterface;
            UseTryAddScoped = useTryAddScoped;
            RegisterAsAllInterfaces = registerAsAllInterfaces;
        }
    }
}
