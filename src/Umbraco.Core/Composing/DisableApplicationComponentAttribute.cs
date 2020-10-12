using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Indicates that an <see cref="IApplicationComponent" /> should be disabled.
    /// </summary>
    /// <remarks>
    /// This attribute has greater priority than the <see cref="EnableAttribute" /> when it's marking the component itself, but lower priority when it's referencing another.
    /// </remarks>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DisableApplicationComponentAttribute : Attribute
    {
        /// <summary>
        /// Gets the disabled type.
        /// </summary>
        /// <value>
        /// The disabled type.
        /// </value>
        public Type DisabledType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableApplicationComponentAttribute" /> class.
        /// </summary>
        /// <param name="disabledType">The <see cref="IApplicationComponent" /> type to disable.</param>
        /// <exception cref="System.ArgumentNullException">disabledType</exception>
        public DisableApplicationComponentAttribute(Type disabledType)
        {
            this.DisabledType = disabledType ?? throw new ArgumentNullException(nameof(disabledType));
        }
    }
}
