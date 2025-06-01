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
        internal string Key { get; }

        /// <summary>
        ///  Indicates whether to use TryAddScoped instead of AddScoped for registration.
        /// </summary>
        internal bool UseTryAddScoped { get; set; }

        public RegisterAsScopedKeyedAttribute(string key, bool useTryAddScoped = false)
        {
            Key = key;
            UseTryAddScoped = useTryAddScoped;
        }
    }
}
