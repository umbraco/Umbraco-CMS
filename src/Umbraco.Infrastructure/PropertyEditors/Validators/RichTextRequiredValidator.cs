using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal class RichTextRequiredValidator : RequiredValidator, IRichTextRequiredValidator
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRequiredValidator> _logger;

    public RichTextRequiredValidator(ILocalizedTextService textService, IJsonSerializer jsonSerializer, ILogger<RichTextRequiredValidator> logger) : base(textService)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public override IEnumerable<ValidationResult> ValidateRequired(object? value, string? valueType) => base.ValidateRequired(GetValue(value), valueType);

    private object? GetValue(object? value)
    {
        if(RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue))
        {
            return richTextEditorValue?.Markup;
        }

        return value;
    }
}
