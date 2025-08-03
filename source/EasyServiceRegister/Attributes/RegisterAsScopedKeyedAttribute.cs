using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a class to be registered as a scoped service with a specific key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsScopedKeyedAttribute : Attribute
    {
        /// <summary>
        ///  Key for the service registration. 
        /// </summary>
        internal object Key { get; }

        /// <summary>
        ///  Indicates whether to use TryAddScoped instead of AddScoped for registration.
        /// </summary>
        internal bool UseTryAddScoped { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        public RegisterAsScopedKeyedAttribute(object key, Type serviceInterface = null, bool useTryAddScoped = false)
        {
            Key = key;
            ServiceInterface = serviceInterface;
            UseTryAddScoped = useTryAddScoped;
        }
    }
}
