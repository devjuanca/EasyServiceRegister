using System;

namespace EasyServiceRegister.Diagnostics
{
    /// <summary>
    /// Contains information about a decorator applied to a service
    /// </summary>
    public class DecoratorInfo
    {
        /// <summary>
        /// The type of the decorator
        /// </summary>
        public Type DecoratorType { get; set; }

        /// <summary>
        /// The order in which the decorator is applied
        /// </summary>
        public int Order { get; set; }
    }
}