using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Indicates that a type should be disabled.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DisableAttribute : Attribute
    {
        /// <summary>
        /// Gets the disabled type, or <c>null</c> if the type it's applied on must be disabled.
        /// </summary>
        /// <value>
        /// The disabled type.
        /// </value>
        public Type DisabledType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableAttribute" /> class.
        /// </summary>
        public DisableAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableAttribute" /> class.
        /// </summary>
        /// <param name="disabledType">The type to disable.</param>
        public DisableAttribute(Type disabledType)
        {
            DisabledType = disabledType;
        }
    }
}
