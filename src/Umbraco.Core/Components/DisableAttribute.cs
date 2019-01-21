using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Indicates that a composer should be disabled.
    /// </summary>
    /// <remarks>
    /// <para>If a type is specified, disables the composer of that type, else disables the composer marked with the attribute.</para>
    /// <para>This attribute is *not* inherited.</para>
    /// <para>This attribute applies to classes only, it is not possible to enable/disable interfaces.</para>
    /// <para>If a composer ends up being both enabled and disabled: attributes marking the composer itself have lower priority
    /// than attributes on *other* composers, eg if a composer declares itself as disabled it is possible to enable it from
    /// another composer. Anything else is unspecified.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DisableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisableAttribute"/> class.
        /// </summary>
        public DisableAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableAttribute"/> class.
        /// </summary>
        public DisableAttribute(Type disabledType)
        {
            DisabledType = disabledType;
        }

        /// <summary>
        /// Gets the disabled type, or null if it is the composer marked with the attribute.
        /// </summary>
        public Type DisabledType { get; }
    }
}
