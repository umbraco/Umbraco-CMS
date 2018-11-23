using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Indicates that a component is required by another component.
    /// </summary>
    /// <remarks>
    /// <para>This attribute is *not* inherited. This means that a component class inheriting from
    /// another component class does *not* inherit its requirements. However, the bootloader checks
    /// the *interfaces* of every component for their requirements, so requirements declared on
    /// interfaces are inherited by every component class implementing the interface.</para>
    /// <para>When targeting a class, indicates a dependency on the component which must be enabled,
    /// unless the requirement has explicitly been declared as weak (and then, only if the component
    /// is enabled).</para>
    /// <para>When targeting an interface, indicates a dependency on enabled components implementing
    /// the interface. It could be no component at all, unless the requirement has explicitly been
    /// declared as strong (and at least one component must be enabled).</para>
    /// </remarks>

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public class RequiredByComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredByComponentAttribute"/> class.
        /// </summary>
        /// <param name="requiringType">The type of the required component.</param>
        public RequiredByComponentAttribute(Type requiringType)
        {
            if (typeof(IUmbracoComponent).IsAssignableFrom(requiringType) == false)
                throw new ArgumentException($"Type {requiringType.FullName} is invalid here because it does not implement {typeof(IUmbracoComponent).FullName}.");
            RequiringType = requiringType;
        }

        /// <summary>
        /// Gets the required type.
        /// </summary>
        public Type RequiringType { get; }
    }
}
