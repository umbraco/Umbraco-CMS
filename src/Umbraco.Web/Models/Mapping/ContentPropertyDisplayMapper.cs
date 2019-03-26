using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates a ContentPropertyDisplay from a Property
    /// </summary>
    internal class ContentPropertyDisplayMapper : ContentPropertyBasicMapper<ContentPropertyDisplay>
    {
        private readonly ILocalizedTextService _textService;

        public ContentPropertyDisplayMapper(IDataTypeService dataTypeService, ILocalizedTextService textService, ILogger logger, PropertyEditorCollection propertyEditors)
            : base(dataTypeService, logger, propertyEditors)
        {
            _textService = textService;
        }
        public override ContentPropertyDisplay Map(Property originalProp, ContentPropertyDisplay dest, MapperContext context)
        {
            var display = base.Map(originalProp, dest, context);

            var config = DataTypeService.GetDataType(originalProp.PropertyType.DataTypeId).Configuration;

            // TODO: IDataValueEditor configuration - general issue
            // GetValueEditor() returns a non-configured IDataValueEditor
            // - for richtext and nested, configuration determines HideLabel, so we need to configure the value editor
            // - could configuration also determines ValueType, everywhere?
            // - does it make any sense to use a IDataValueEditor without configuring it?

            // configure the editor for display with configuration
            var valEditor = display.PropertyEditor.GetValueEditor(config);

            //set the display properties after mapping
            display.Alias = originalProp.Alias;
            display.Description = originalProp.PropertyType.Description;
            display.Label = originalProp.PropertyType.Name;
            display.HideLabel = valEditor.HideLabel;

            //add the validation information
            display.Validation.Mandatory = originalProp.PropertyType.Mandatory;
            display.Validation.Pattern = originalProp.PropertyType.ValidationRegExp;

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
                display.Config = display.PropertyEditor.GetConfigurationEditor().ToValueEditor(config);
                display.View = valEditor.View;
            }

            //Translate
            display.Label = _textService.UmbracoDictionaryTranslate(display.Label);
            display.Description = _textService.UmbracoDictionaryTranslate(display.Description);

            return display;
        }
    }
}
