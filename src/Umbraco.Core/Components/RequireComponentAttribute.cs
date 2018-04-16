using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Indicates that a component requires another component.
    /// </summary>
    /// <remarks>
    /// <para>This attribute is *not* inherited. This means that a component class inheriting from
    /// another component class does *not* inherit its requirements. However, the bootloader checks
    /// the *interfaces* of every component for their requirements, so requirements declared on
    /// interfaces are inherited by every component class implementing the interface.</para>
    /// <para>When targetting a class, indicates a dependency on the component which must be enabled,
    /// unless the requirement has explicitely been declared as weak (and then, only if the component
    /// is enabled).</para>
    /// <para>When targetting an interface, indicates a dependency on enabled components implementing
    /// the interface. It could be no component at all, unless the requirement has explicitely been
    /// declared as strong (and at least one component must be enabled).</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public class RequireComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireComponentAttribute"/> class.
        /// </summary>
        /// <param name="requiredType">The type of the required component.</param>
        public RequireComponentAttribute(Type requiredType)
        {
            if (typeof(IUmbracoComponent).IsAssignableFrom(requiredType) == false)
                throw new ArgumentException($"Type {requiredType.FullName} is invalid here because it does not implement {typeof(IUmbracoComponent).FullName}.");
            RequiredType = requiredType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireComponentAttribute"/> class.
        /// </summary>
        /// <param name="requiredType">The type of the required component.</param>
        /// <param name="weak">A value indicating whether the requirement is weak.</param>
        public RequireComponentAttribute(Type requiredType, bool weak)
            : this(requiredType)
        {
            Weak = weak;
        }

        /// <summary>
        /// Gets the required type.
        /// </summary>
        public Type RequiredType { get; }

        /// <summary>
        /// Gets a value indicating whether the requirement is weak.
        /// </summary>
        /// <remarks>Returns <c>true</c> if the requirement is weak (requires the other component if it
        /// is enabled), <c>false</c> if the requirement is strong (requires the other component to be
        /// enabled), and <c>null</c> if unspecified, in which case it is strong for classes and weak for
        /// interfaces.</remarks>
        public bool? Weak { get; }
    }
}
