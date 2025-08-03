using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a class to be registered as a singleton service with a specific key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsSingletonKeyedAttribute : Attribute
    {
        /// <summary>
        /// Key for the service registration.
        /// </summary>
        internal object Key { get; }

        /// <summary>
        /// Indicates whether to use TryAddSingleton instead of AddSingleton for registration.
        /// </summary>
        internal bool UseTryAddSingleton { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        public RegisterAsSingletonKeyedAttribute(object key, Type serviceInterface = null, bool useTryAdd = false)
        {
            Key = key;
            ServiceInterface = serviceInterface;
            UseTryAddSingleton = useTryAdd;
        }
    }
}
