using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal class RichTextRegexValidator : RegexValidator, IRichTextRegexValidator
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRegexValidator> _logger;

    public RichTextRegexValidator(ILocalizedTextService textService, IJsonSerializer jsonSerializer, ILogger<RichTextRegexValidator> logger)
        : this(textService, string.Empty, jsonSerializer, logger)
    {
    }

    public RichTextRegexValidator(ILocalizedTextService textService, string regex, IJsonSerializer jsonSerializer, ILogger<RichTextRegexValidator> logger) : base(textService, regex)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public override IEnumerable<ValidationResult> ValidateFormat(object? value, string? valueType, string format) => base.ValidateFormat(GetValue(value), valueType, format);

    private object? GetValue(object? value)
    {
        if (RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue))
        {
            return richTextEditorValue?.Markup;
        }

        return value;
    }
}
