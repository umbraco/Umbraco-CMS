using System;
using Umbraco.Core.Models;
using Umbraco.Tests.CodeFirst.Definitions;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyTypeAttribute : PropertyTypeConventionAttribute
    {
        public PropertyTypeAttribute(Type type)
        {
            Type = type;
            PropertyGroup = "Generic Properties";
            Mandatory = false;
            ValidationRegExp = string.Empty;
            DatabaseType = DataTypeDatabaseType.Nvarchar;
        }

        /// <summary>
        /// Gets or sets the Type of the DataType
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets or sets the Database Type to use for the chosen DataType.
        /// </summary>
        /// <remarks>
        /// Please note that the DatabaseType only needs to be set when 
        /// creating a new DataType definition.
        /// </remarks>
        public DataTypeDatabaseType DatabaseType { get; set; }

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

        public override PropertyDefinition GetPropertyConvention()
        {
            var definition = new PropertyDefinition();

            definition.Mandatory = Mandatory;
            definition.ValidationRegExp = string.IsNullOrEmpty(ValidationRegExp) ? ValidationRegExp : string.Empty;
            definition.PropertyGroup = string.IsNullOrEmpty(PropertyGroup) ? "Generic Properties" : PropertyGroup;
            definition.DataTypeDefinition = Conventions.GetDataTypeDefinitionByAttributeOrType(this, Type);

            return definition;
        }
    }
}