using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Indicates that a type should be enabled.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class EnableAttribute : Attribute
    {
        /// <summary>
        /// Gets the enabled type, or <c>null</c> if the type it's applied on must be enabled.
        /// </summary>
        /// <value>
        /// The enabled type.
        /// </value>
        public Type EnabledType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableAttribute" /> class.
        /// </summary>
        public EnableAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableAttribute" /> class.
        /// </summary>
        /// <param name="enabledType">The type to enable.</param>
        public EnableAttribute(Type enabledType)
        {
            EnabledType = enabledType;
        }
    }
}
