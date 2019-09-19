using System;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDisplay from a Property
    /// </summary>
    internal class ContentPropertyDisplayConverter : ContentPropertyBasicConverter<ContentPropertyDisplay>
    {
        private readonly ILocalizedTextService _textService;

        public ContentPropertyDisplayConverter(IDataTypeService dataTypeService, ILocalizedTextService textService, IEntityService entityService)
            : base(dataTypeService, entityService)
        {
            _textService = textService;
        }
        public override ContentPropertyDisplay Convert(ResolutionContext context)
        {
            var display = base.Convert(context);

            var originalProperty = context.SourceValue as Property;
            if (originalProperty == null)
                throw new InvalidOperationException("Source value is not a property.");

            var dataTypeService = DataTypeService;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(originalProperty.PropertyType.DataTypeDefinitionId);

            //configure the editor for display with the pre-values
            var valEditor = display.PropertyEditor.ValueEditor;
            valEditor.ConfigureForDisplay(preVals);

            //set the display properties after mapping
            display.Alias = originalProperty.Alias;
            display.Description = originalProperty.PropertyType.Description;
            display.Label = originalProperty.PropertyType.Name;
            display.HideLabel = valEditor.HideLabel;

            //add the validation information
            display.Validation.Mandatory = originalProperty.PropertyType.Mandatory;
            display.Validation.Pattern = originalProperty.PropertyType.ValidationRegExp;

            if (display.PropertyEditor == null)
            {
                //display.Config = PreValueCollection.AsDictionary(preVals);
                //if there is no property editor it means that it is a legacy data type
                // we cannot support editing with that so we'll just render the readonly value view.
                display.View = "views/propertyeditors/readonlyvalue/readonlyvalue.html";
            }
            else
            {
                //let the property editor format the pre-values
                display.Config = display.PropertyEditor.PreValueEditor.ConvertDbToEditor(display.PropertyEditor.DefaultPreValues, preVals);
                display.View = valEditor.View;
            }

            //Translate
            display.Label = _textService.UmbracoDictionaryTranslate(display.Label);
            display.Description = _textService.UmbracoDictionaryTranslate(display.Description);

            return display;
        }
    }
}
