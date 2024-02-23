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

        // v13 to v14 merge note: because of changes in the IDataType we can not use the optimized IDataTypeConfigurationCache here
        // todo: make sure this (possible) performance degradation isn't serious
        IDataType? dataType = DataTypeService.GetDataType(originalProp.PropertyType.DataTypeId);

        // TODO: IDataValueEditor configuration - general issue
        // GetValueEditor() returns a non-configured IDataValueEditor
        // - for richtext and nested, configuration determines HideLabel, so we need to configure the value editor
        // - could configuration also determines ValueType, everywhere?
        // - does it make any sense to use a IDataValueEditor without configuring it?

        // set the display properties after mapping
        dest.Alias = originalProp.Alias;
        dest.Description = originalProp.PropertyType?.Description;
        dest.Label = originalProp.PropertyType?.Name;

        // Set variation, the frontend needs this to determine if the content varies by segment
        dest.Variations = originalProp.PropertyType?.Variations ?? ContentVariation.Nothing;

        // add the validation information
        dest.Validation.Mandatory = originalProp.PropertyType?.Mandatory ?? false;
        dest.Validation.MandatoryMessage = originalProp.PropertyType?.MandatoryMessage;
        dest.Validation.Pattern = originalProp.PropertyType?.ValidationRegExp;
        dest.Validation.PatternMessage = originalProp.PropertyType?.ValidationRegExpMessage;

        if (dest.PropertyEditor != null)
        {
            // let the property editor format the pre-values
            if (dataType != null)
            {
                dest.Config = dest.PropertyEditor.GetConfigurationEditor().ToValueEditor(dataType.ConfigurationData);
            }
        }

        // Translate
        dest.Label = _textService.UmbracoDictionaryTranslate(_cultureDictionary, dest.Label);
        dest.Description = _textService.UmbracoDictionaryTranslate(_cultureDictionary, dest.Description);
    }
}
