using System;
using System.Collections.Generic;
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
        public T Convert(ResolutionContext context)
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

            // Complex properties such as Nested Content do not need to be mapped for simpler things like list views,
            // where they will not make sense to use anyways. To avoid having to do unnecessary mapping on large
            // collections of items in list views - we allow excluding mapping of certain properties.
            var excludeComplexProperties = false;
            if (context.Options.Items.ContainsKey("ExcludeComplexProperties"))
            {
                excludeComplexProperties = System.Convert.ToBoolean(context.Options.Items["ExcludeComplexProperties"]);
            }
            if (excludeComplexProperties == false || ComplexPropertyTypeAliases.Contains(property.PropertyType.PropertyEditorAlias) == false)
            {
                result.Value = editor.ValueEditor.ConvertDbToEditor(property, property.PropertyType, DataTypeService);
            }
            
            return result;
        }
    }
}
