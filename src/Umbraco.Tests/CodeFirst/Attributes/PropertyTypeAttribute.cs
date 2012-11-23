using System;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyTypeAttribute : Attribute
    {
        public PropertyTypeAttribute(Type type)
        {
            Type = type;
            PropertyGroup = "Generic Properties";
            Mandatory = false;
            ValidationRegExp = string.Empty;
        }

        /// <summary>
        /// Gets or sets the Type of the DataType
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets or sets the Name of the PropertyGroup that this PropertyType belongs to
        /// </summary>
        public string PropertyGroup { get; set; }

        /// <summary>
        /// Boolean indicating that a value is required for this PropertyType
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Regular Expression for validating PropertyType's values
        /// </summary>
        public string ValidationRegExp { get; set; }
    }
}