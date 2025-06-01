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
        public RegisterAsSingletonAttribute(bool useTryAddSingleton = false)
        {
            UseTryAddSingleton = useTryAddSingleton;
        }
    }
}
