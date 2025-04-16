using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class RichTextEditorBlockValidator: BlockEditorValidatorBase<RichTextBlockValue, RichTextBlockLayoutItem>
{
    private readonly BlockEditorValues<RichTextBlockValue, RichTextBlockLayoutItem> _blockEditorValues;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger _logger;

    public RichTextEditorBlockValidator(
        IPropertyValidationService propertyValidationService,
        BlockEditorValues<RichTextBlockValue, RichTextBlockLayoutItem> blockEditorValues,
        IBlockEditorElementTypeCache elementTypeCache,
        IJsonSerializer jsonSerializer,
        ILogger logger)
        : base(propertyValidationService, elementTypeCache)
    {
        _blockEditorValues = blockEditorValues;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    protected override string ContentDataGroupJsonPath =>
        $"{nameof(RichTextEditorValue.Blocks).ToFirstLowerInvariant()}.{base.ContentDataGroupJsonPath}";

    protected override string SettingsDataGroupJsonPath =>
        $"{nameof(RichTextEditorValue.Blocks).ToFirstLowerInvariant()}.{base.SettingsDataGroupJsonPath}";

    protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value, PropertyValidationContext validationContext)
    {
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue);
        if (richTextEditorValue?.Blocks is null)
        {
            return Array.Empty<ElementTypeValidationModel>();
        }

        BlockEditorData<RichTextBlockValue, RichTextBlockLayoutItem>? blockEditorData = _blockEditorValues.ConvertAndClean(richTextEditorValue.Blocks);
        return blockEditorData is not null
            ? GetBlockEditorDataValidation(blockEditorData, validationContext)
            : Array.Empty<ElementTypeValidationModel>();
    }
}
