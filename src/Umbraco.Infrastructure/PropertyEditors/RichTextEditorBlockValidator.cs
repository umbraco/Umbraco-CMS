using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class RichTextEditorBlockValidator : BlockEditorValidatorBase
{
    private readonly BlockEditorValues _blockEditorValues;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger _logger;

    public RichTextEditorBlockValidator(
        IPropertyValidationService propertyValidationService,
        BlockEditorValues blockEditorValues,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer,
        ILogger logger)
        : base(propertyValidationService, contentTypeService)
    {
        _blockEditorValues = blockEditorValues;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    protected override IEnumerable<ElementTypeValidationModel> GetElementTypeValidation(object? value)
    {
        RichTextEditorValue? richTextEditorValue = value.TryParseRichTextEditorValue(_jsonSerializer, _logger);
        if (richTextEditorValue?.Blocks is null)
        {
            return Array.Empty<ElementTypeValidationModel>();
        }

        BlockEditorData? blockEditorData = _blockEditorValues.ConvertAndClean(richTextEditorValue.Blocks);
        return blockEditorData is not null
            ? GetBlockEditorDataValidation(blockEditorData)
            : Array.Empty<ElementTypeValidationModel>();
    }
}
