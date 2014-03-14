using System;
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
    internal class ContentPropertyBasicConverter<T> : TypeConverter<Property, T>
        where T : ContentPropertyBasic, new()
    {
        protected Lazy<IDataTypeService> DataTypeService { get; private set; }

        public ContentPropertyBasicConverter(Lazy<IDataTypeService> dataTypeService)
        {
            DataTypeService = dataTypeService;
        }

        /// <summary>
        /// Assigns the PropertyEditor, Id, Alias and Value to the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected override T ConvertCore(Property property)
        {
            var editor = PropertyEditorResolver.Current.GetByAlias(property.PropertyType.PropertyEditorAlias);
            if (editor == null)
            {
                LogHelper.Error<ContentPropertyBasicConverter<T>>(
                    "No property editor found, converting to a Label",
                    new NullReferenceException("The property editor with alias " + property.PropertyType.PropertyEditorAlias + " does not exist"));

                editor = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias);
            }
            var result = new T
                {
                    Id = property.Id,
                    Value = editor.ValueEditor.ConvertDbToEditor(property, property.PropertyType, DataTypeService.Value),
                    Alias = property.Alias, 
                    PropertyEditor = editor,
                    Editor = editor.Alias
                };

            return result;
        }
    }
}