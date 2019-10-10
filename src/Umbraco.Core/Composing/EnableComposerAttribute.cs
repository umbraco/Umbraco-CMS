using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Indicates that an <see cref="IComposer" /> should be enabled.
    /// </summary>
    /// <remarks>
    /// This attribute has greater priority than the <see cref="DisableAttribute" /> when it's marking the composer itself, but lower priority when it's referencing another.
    /// </remarks>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class EnableComposerAttribute : Attribute
    {
        /// <summary>
        /// Gets the enabled type.
        /// </summary>
        /// <value>
        /// The enabled type.
        /// </value>
        public Type EnabledType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableComposerAttribute" /> class.
        /// </summary>
        /// <param name="enabledType">The <see cref="IComposer" /> type to enable.</param>
        /// <exception cref="System.ArgumentNullException">enabledType</exception>
        public EnableComposerAttribute(Type enabledType)
        {
            EnabledType = enabledType ?? throw new ArgumentNullException(nameof(enabledType));
        }
    }
}
