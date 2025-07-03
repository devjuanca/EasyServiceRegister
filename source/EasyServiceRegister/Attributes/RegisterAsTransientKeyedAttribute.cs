using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a class to be registered as a transient service with a specific key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsTransientKeyedAttribute : Attribute
    {
        /// <summary>
        /// Key for the service registration.
        /// </summary>
        internal string Key { get; }

        /// <summary>
        /// Indicates whether to use TryAddTransient instead of AddTransient for registration.
        /// </summary>
        internal bool UseTryAddTransient { get; set; }

        /// <summary>
        /// Indicates the service interface type to register the class as.
        /// </summary>
        internal Type ServiceInterface { get; set; }

        public RegisterAsTransientKeyedAttribute(string key, Type serviceInterface = null, bool useTryAdd = false)
        {
            Key = key;
            ServiceInterface = serviceInterface;
            UseTryAddTransient = useTryAdd;
        }
    }
}
