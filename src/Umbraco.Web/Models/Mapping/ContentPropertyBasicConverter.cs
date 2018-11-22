using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a base generic ContentPropertyBasic from a Property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ContentPropertyBasicConverter<T> : ITypeConverter<Property, T>
        where T : ContentPropertyBasic, new()
    {
        protected IDataTypeService DataTypeService { get; private set; }

        private static readonly List<string> ComplexPropertyTypeAliases = new List<string> {"Umbraco.NestedContent"};

        public ContentPropertyBasicConverter(IDataTypeService dataTypeService)
        {
            DataTypeService = dataTypeService;
        }

        /// <summary>
        /// Assigns the PropertyEditor, Id, Alias and Value to the property
        /// </summary>
        /// <returns></returns>
        public virtual T Convert(ResolutionContext context)
        {
            var property = context.SourceValue as Property;
            if (property == null)
                throw new InvalidOperationException("Source value is not a property.");

            var editor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);
            if (editor == null)
            {
                LogHelper.Error<ContentPropertyBasicConverter<T>>(
                    "No property editor found, converting to a Label",
                    new NullReferenceException("The property editor with alias " +
                                               property.PropertyType.PropertyEditorAlias + " does not exist"));

                editor = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias);
            }

            var result = new T
            {
                Id = property.Id,
                Alias = property.Alias,
                PropertyEditor = editor,
                Editor = editor.Alias
            };

            // if there's a set of property aliases specified, we will check if the current property's value should be mapped.
            // if it isn't one of the ones specified in 'includeProperties', we will just return the result without mapping the Value.
            if (context.Options.Items.ContainsKey("IncludeProperties"))
            {
                var includeProperties = context.Options.Items["IncludeProperties"] as IEnumerable<string>;
                if (includeProperties != null && includeProperties.Contains(property.Alias) == false)
                {
                    return result;
                }
            }

            // if no 'IncludeProperties' were specified or this property is set to be included - we will map the value and return.
            result.Value = editor.ValueEditor.ConvertDbToEditor(property, property.PropertyType, DataTypeService);
            return result;
        }
    }
}
