using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Indicates that a composer should be enabled.
    /// </summary>
    /// <remarks>
    /// <para>If a type is specified, enables the composer of that type, else enables the composer marked with the attribute.</para>
    /// <para>This attribute is *not* inherited.</para>
    /// <para>This attribute applies to classes only, it is not possible to enable/disable interfaces.</para>
    /// <para>If a composer ends up being both enabled and disabled: attributes marking the composer itself have lower priority
    /// than attributes on *other* composers, eg if a composer declares itself as disabled it is possible to enable it from
    /// another composer. Anything else is unspecified.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class EnableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnableAttribute"/> class.
        /// </summary>
        public EnableAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableAttribute"/> class.
        /// </summary>
        public EnableAttribute(Type enabledType)
        {
            EnabledType = enabledType;
        }

        /// <summary>
        /// Gets the enabled type, or null if it is the composer marked with the attribute.
        /// </summary>
        public Type EnabledType { get; }
    }
}
