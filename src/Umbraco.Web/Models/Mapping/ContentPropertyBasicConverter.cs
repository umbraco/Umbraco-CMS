using System;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Core.Models.ContentVariation;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a base generic ContentPropertyBasic from a Property
    /// </summary>
    internal class ContentPropertyBasicConverter<TDestination> : ITypeConverter<Property, TDestination>
        where TDestination : ContentPropertyBasic, new()
    {
        private readonly ILogger _logger;
        private readonly PropertyEditorCollection _propertyEditors;
        protected IDataTypeService DataTypeService { get; }

        public ContentPropertyBasicConverter(IDataTypeService dataTypeService, ILogger logger, PropertyEditorCollection propertyEditors)
        {
            _logger = logger;
            _propertyEditors = propertyEditors;
            DataTypeService = dataTypeService;
        }

        /// <summary>
        /// Assigns the PropertyEditor, Id, Alias and Value to the property
        /// </summary>
        /// <returns></returns>
        public virtual TDestination Convert(Property property, TDestination dest, ResolutionContext context)
        {
            var editor = _propertyEditors[property.PropertyType.PropertyEditorAlias];
            if (editor == null)
            {
                _logger.Error<ContentPropertyBasicConverter<TDestination>>(
                    "No property editor found, converting to a Label",
                    new NullReferenceException("The property editor with alias " + property.PropertyType.PropertyEditorAlias + " does not exist"));

                editor = _propertyEditors[Constants.PropertyEditors.Aliases.NoEdit];
            }

            var languageId = context.GetLanguageId();
            
            if (!languageId.HasValue && property.PropertyType.Variations == ContentVariation.CultureNeutral)
            {
                //a language Id needs to be set for a property type that can be varried by language
                throw new InvalidOperationException($"No languageId found in mapping operation when one is required for the culture neutral property type {property.PropertyType.Alias}");
            }

            var result = new TDestination
                {
                    Id = property.Id,
                    Value = editor.GetValueEditor().ToEditor(property, DataTypeService, languageId),
                    Alias = property.Alias,
                    PropertyEditor = editor,
                    Editor = editor.Alias
                };

            return result;
        }
    }
}
