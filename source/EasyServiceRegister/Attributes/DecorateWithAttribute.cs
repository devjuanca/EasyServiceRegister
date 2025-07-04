using System;

namespace EasyServiceRegister.Attributes
{
    /// <summary>
    /// Marks a service implementation to be decorated with the specified decorator type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DecorateWithAttribute : Attribute
    {
        /// <summary>
        /// The type of the decorator to apply to the service.
        /// </summary>
        internal Type DecoratorType { get; }

        /// <summary>
        /// The order in which multiple decorators should be applied (lower numbers are applied first).
        /// </summary>
        internal int Order { get; }

        /// <summary>
        /// Specifies a decorator to be applied to the service.
        /// </summary>
        /// <param name="decoratorType">The type of the decorator. Must implement the same interface as the decorated service.</param>
        /// <param name="order">The order in which to apply multiple decorators (lower numbers wrap first).</param>
        public DecorateWithAttribute(Type decoratorType, int order = 0)
        {
            DecoratorType = decoratorType ?? throw new ArgumentNullException(nameof(decoratorType));
            Order = order;
        }
    }
}