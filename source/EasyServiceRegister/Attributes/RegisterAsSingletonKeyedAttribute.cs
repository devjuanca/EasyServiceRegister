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
        internal string Key { get; }

        /// <summary>
        /// Indicates whether to use TryAddSingleton instead of AddSingleton for registration.
        /// </summary>
        internal bool UseTryAddSingleton { get; set; }

        public RegisterAsSingletonKeyedAttribute(string key, bool useTryAdd = false)
        {
            Key = key;
            UseTryAddSingleton = useTryAdd;
        }
    }
}
