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

        public RegisterAsScopedAttribute(bool useTryAddScoped = false)
        {
            UseTryAddScoped = useTryAddScoped;
        }
    }
}
