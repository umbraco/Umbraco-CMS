using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Indicates the CLR type of property object values returned by a converter.
    /// </summary>
    /// <remarks>Use this attribute to mark property values converters.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PropertyValueTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueTypeAttribute"/> class with a type.
        /// </summary>
        /// <param name="type">The type.</param>
        public PropertyValueTypeAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public Type Type { get; private set; }
    }
}
