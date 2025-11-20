using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal sealed class RichTextRequiredValidator : RequiredValidator, IRichTextRequiredValidator
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRequiredValidator> _logger;

    public RichTextRequiredValidator(IJsonSerializer jsonSerializer, ILogger<RichTextRequiredValidator> logger) : base()
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
