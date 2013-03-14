using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.interfaces;

namespace Umbraco.Tests.CodeFirst.Definitions
{
    public static class Conventions
    {
        /// <summary>
        /// Convention to get a DataTypeDefinition from the PropertyTypeAttribute or the type of the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDataTypeDefinition GetDataTypeDefinitionByAttributeOrType(PropertyTypeAttribute attribute, Type type)
        {
            if (attribute != null)
            {
                var instance = Activator.CreateInstance(attribute.Type);
                var dataType = instance as IDataType;
                var definition = GetDataTypeByControlId(dataType.Id);
                //If the DataTypeDefinition doesn't exist we create a new one
                if (definition == null)
                {
                    definition = new DataTypeDefinition(-1, dataType.Id)
                                     {
                                         DatabaseType = attribute.DatabaseType,
                                         Name = dataType.DataTypeName
                                     };
					ApplicationContext.Current.Services.DataTypeService.Save(definition, 0);
                }
                return definition;
            }

            return GetPredefinedDataTypeDefinitionByType(type);
        }

        /// <summary>
        /// Convention to get predefined DataTypeDefinitions based on the Type of the property
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDataTypeDefinition GetPredefinedDataTypeDefinitionByType(Type type)
        {
            if (type == typeof(bool))
            {
                return GetDataTypeByControlId(new Guid(Constants.PropertyEditors.TrueFalse));// Yes/No DataType
            }
            if (type == typeof(int))
            {
                return GetDataTypeByControlId(new Guid(Constants.PropertyEditors.Integer));// Number DataType
            }
            if (type == typeof(DateTime))
            {
                return GetDataTypeByControlId(new Guid(Constants.PropertyEditors.Date));// Date Picker DataType
            }

            return GetDataTypeByControlId(new Guid(Constants.PropertyEditors.Textbox));// Standard textfield
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeDefinition"/> from the DataTypeService by its control Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IDataTypeDefinition GetDataTypeByControlId(Guid id)
        {
			var definitions = ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByControlId(id);
            return definitions.FirstOrDefault();
        }

        /// <summary>
        /// Creates a new DataTypeDefinition based on the Type in the PropertyTypeAttribute
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="dataTypeDefinitionName"></param>
        /// <returns></returns>
        public static IDataTypeDefinition CreateDataTypeDefinitionFromAttribute(PropertyTypeAttribute attribute, string dataTypeDefinitionName)
        {
            var instance = Activator.CreateInstance(attribute.Type);
            var dataType = instance as IDataType;

            var definition = new DataTypeDefinition(-1, dataType.Id)
                                 {
                                     DatabaseType = attribute.DatabaseType,
                                     Name = dataTypeDefinitionName
                                 };
			ApplicationContext.Current.Services.DataTypeService.Save(definition, 0);
            return definition;
        }

        /// <summary>
        /// Creates a PreValue for a <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitionId"></param>
        /// <param name="value"></param>
        /// <param name="sortOrder"></param>
        /// <param name="alias"></param>
        public static void CreatePrevalueForDataTypeDefinition(int dataTypeDefinitionId, string value, int sortOrder, string alias)
        {
            var poco = new DataTypePreValueDto
                           {
                               Alias = alias, 
                               DataTypeNodeId = dataTypeDefinitionId, 
                               SortOrder = sortOrder, 
                               Value = value
                           };

            ApplicationContext.Current.DatabaseContext.Database.Insert(poco);
        }

        /// <summary>
        /// Convention to get the Alias of the PropertyType from the AliasAttribute or the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetPropertyTypeAlias(AliasAttribute attribute, string propertyName)
        {
            return attribute == null ? propertyName.ToUmbracoAlias() : attribute.Alias;
        }

        /// <summary>
        /// Convention to get the Name of the PropertyType from the AliasAttribute or the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetPropertyTypeName(AliasAttribute attribute, string propertyName)
        {
            if (attribute == null)
                return propertyName.SplitPascalCasing();

            return string.IsNullOrEmpty(attribute.Name) ? propertyName.SplitPascalCasing() : attribute.Name;
        }
    }
}