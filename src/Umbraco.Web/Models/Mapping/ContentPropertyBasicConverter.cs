using System;
using System.Collections.Generic;
using System.Linq;
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
                    new NullReferenceException("The property editor with alias " + property.PropertyType.PropertyEditorAlias + " does not exist"),
                    "No property editor '{PropertyEditorAlias}' found, converting to a Label",
                    property.PropertyType.PropertyEditorAlias);

                editor = _propertyEditors[Constants.PropertyEditors.Aliases.Label];
            }

            var result = new TDestination
                {
                    Id = property.Id,
                    Alias = property.Alias,
                    PropertyEditor = editor,
                    Editor = editor.Alias
                };

            // if there's a set of property aliases specified, we will check if the current property's value should be mapped.
            // if it isn't one of the ones specified in 'includeProperties', we will just return the result without mapping the Value.
            var includedProperties = context.Options.GetIncludedProperties();
            if (includedProperties != null && !includedProperties.Contains(property.Alias))
                return result;

            //Get the culture from the context which will be set during the mapping operation for each property
            var culture = context.Options.GetCulture();

            //a culture needs to be in the context for a property type that can vary
            if (culture == null && property.PropertyType.VariesByCulture())
                throw new InvalidOperationException($"No culture found in mapping operation when one is required for the culture variant property type {property.PropertyType.Alias}");

            //set the culture to null if it's an invariant property type
            culture = !property.PropertyType.VariesByCulture() ? null : culture;

            result.Culture = culture;

            // if no 'IncludeProperties' were specified or this property is set to be included - we will map the value and return.
            result.Value = editor.GetValueEditor().ToEditor(property, DataTypeService, culture);
            return result;
        }
    }
}
