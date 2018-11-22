using System;
using Umbraco.Core.Models;
using Umbraco.Tests.CodeFirst.Definitions;
using umbraco.editorControls.numberfield;

namespace Umbraco.Tests.CodeFirst.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class Numeric : PropertyTypeAttribute
    {
        public Numeric(string dataTypeName)
            : base(typeof(IDataTypenteger))
        {
            DataTypeName = dataTypeName;
            DatabaseType = DataTypeDatabaseType.Integer;
        }

        public string DataTypeName { get; set; }

        public string PreValue { get; set; }

        public override PropertyDefinition GetPropertyConvention()
        {
            var definition = new PropertyDefinition();

            definition.Mandatory = Mandatory;
            definition.ValidationRegExp = string.IsNullOrEmpty(ValidationRegExp) ? ValidationRegExp : string.Empty;
            definition.PropertyGroup = string.IsNullOrEmpty(PropertyGroup) ? "Generic Properties" : PropertyGroup;
            definition.DataTypeDefinition = Conventions.CreateDataTypeDefinitionFromAttribute(this, DataTypeName);

            if(string.IsNullOrEmpty(PreValue) == false)
            {
                //TODO - test inserting a prevalue when a DataTypeDefinition has been created, as its currently throwing a foreignkey constraint error.
                Conventions.CreatePrevalueForDataTypeDefinition(definition.DataTypeDefinition.Id, PreValue, 0, string.Empty);
            }

            return definition;
        }
    }
}