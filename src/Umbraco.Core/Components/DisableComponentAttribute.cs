using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Indicates that a component should be disabled.
    /// </summary>
    /// <remarks>
    /// <para>If a type is specified, disables the component of that type, else disables the component marked with the attribute.</para>
    /// <para>This attribute is *not* inherited.</para>
    /// <para>This attribute applies to classes only, it is not possible to enable/disable interfaces.</para>
    /// <para>If a component ends up being both enabled and disabled: attributes marking the component itself have lower priority
    /// than attributes on *other* components, eg if a component declares itself as disabled it is possible to enable it from
    /// another component. Anything else is unspecified.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DisableComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisableComponentAttribute"/> class.
        /// </summary>
        public DisableComponentAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableComponentAttribute"/> class.
        /// </summary>
        public DisableComponentAttribute(Type disabledType)
        {
            DisabledType = disabledType;
        }

        /// <summary>
        /// Gets the disabled type, or null if it is the component marked with the attribute.
        /// </summary>
        public Type DisabledType { get; }
    }
}
