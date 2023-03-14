// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Creates a ContentPropertyDisplay from a Property
/// </summary>
internal class ContentPropertyDisplayMapper : ContentPropertyBasicMapper<ContentPropertyDisplay>
{
    private readonly ICultureDictionary _cultureDictionary;
    private readonly ILocalizedTextService _textService;

    public ContentPropertyDisplayMapper(
        ICultureDictionary cultureDictionary,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        ILocalizedTextService textService,
        ILogger<ContentPropertyDisplayMapper> logger,
        PropertyEditorCollection propertyEditors)
        : base(dataTypeService, entityService, logger, propertyEditors)
    {
        _cultureDictionary = cultureDictionary;
        _textService = textService;
    }

    public override void Map(IProperty originalProp, ContentPropertyDisplay dest, MapperContext context)
    {
        base.Map(originalProp, dest, context);

        var config = originalProp.PropertyType is null
            ? null
            : DataTypeService.GetDataType(originalProp.PropertyType.DataTypeId)?.Configuration;

        // TODO: IDataValueEditor configuration - general issue
        // GetValueEditor() returns a non-configured IDataValueEditor
        // - for richtext and nested, configuration determines HideLabel, so we need to configure the value editor
        // - could configuration also determines ValueType, everywhere?
        // - does it make any sense to use a IDataValueEditor without configuring it?

        // configure the editor for display with configuration
        IDataValueEditor? valEditor = dest.PropertyEditor?.GetValueEditor(config);

        // set the display properties after mapping
        dest.Alias = originalProp.Alias;
        dest.Description = originalProp.PropertyType?.Description;
        dest.Label = originalProp.PropertyType?.Name;
        dest.HideLabel = valEditor?.HideLabel ?? false;
        dest.LabelOnTop = originalProp.PropertyType?.LabelOnTop;

        // Set variation, the frontend needs this to determine if the content varies by segment
        dest.Variations = originalProp.PropertyType?.Variations ?? ContentVariation.Nothing;

        // add the validation information
        dest.Validation.Mandatory = originalProp.PropertyType?.Mandatory ?? false;
        dest.Validation.MandatoryMessage = originalProp.PropertyType?.MandatoryMessage;
        dest.Validation.Pattern = originalProp.PropertyType?.ValidationRegExp;
        dest.Validation.PatternMessage = originalProp.PropertyType?.ValidationRegExpMessage;

        if (dest.PropertyEditor == null)
        {
            // display.Config = PreValueCollection.AsDictionary(preVals);
            // if there is no property editor it means that it is a legacy data type
            // we cannot support editing with that so we'll just render the readonly value view.
            dest.View = "views/propertyeditors/readonlyvalue/readonlyvalue.html";
        }
        else
        {
            // let the property editor format the pre-values
            dest.Config = dest.PropertyEditor.GetConfigurationEditor().ToValueEditor(config);
            dest.View = valEditor?.View;
        }

        // Translate
        dest.Label = _textService.UmbracoDictionaryTranslate(_cultureDictionary, dest.Label);
        dest.Description = _textService.UmbracoDictionaryTranslate(_cultureDictionary, dest.Description);
    }
}
