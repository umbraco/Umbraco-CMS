using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a base generic ContentPropertyBasic from a Property
    /// </summary>
    internal class ContentPropertyBasicConverter<TDestination> : ITypeConverter<Property, TDestination>
        where TDestination : ContentPropertyBasic, new()
    {
        protected IDataTypeService DataTypeService { get; }

        public ContentPropertyBasicConverter(IDataTypeService dataTypeService)
        {
            DataTypeService = dataTypeService;
        }

        /// <summary>
        /// Assigns the PropertyEditor, Id, Alias and Value to the property
        /// </summary>
        /// <returns></returns>
        public virtual TDestination Convert(Property property, TDestination dest, ResolutionContext context)
        {
            var editor = Current.PropertyEditors[property.PropertyType.PropertyEditorAlias];
            if (editor == null)
            {
                Current.Logger.Error<ContentPropertyBasicConverter<TDestination>>(
                    "No property editor found, converting to a Label",
                    new NullReferenceException("The property editor with alias " + property.PropertyType.PropertyEditorAlias + " does not exist"));

                editor = Current.PropertyEditors[Constants.PropertyEditors.Aliases.NoEdit];
            }
            var result = new TDestination
                {
                    Id = property.Id,
                    Value = editor.GetValueEditor().ToEditor(property, DataTypeService),
                    Alias = property.Alias,
                    PropertyEditor = editor,
                    Editor = editor.Alias
                };

            return result;
        }
    }
}
