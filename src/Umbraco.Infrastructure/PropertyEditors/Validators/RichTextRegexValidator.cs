using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal class RichTextRegexValidator : IRichTextRegexValidator
{
    private readonly RegexValidator _regexValidator;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRegexValidator> _logger;

    public RichTextRegexValidator(
        IJsonSerializer jsonSerializer,
        ILogger<RichTextRegexValidator> logger)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _regexValidator = new RegexValidator();
    }

    public IEnumerable<ValidationResult> ValidateFormat(object? value, string? valueType, string format) => _regexValidator.ValidateFormat(GetValue(value), valueType, format);

    private object? GetValue(object? value) =>
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue)
            ? richTextEditorValue?.Markup
            : value;
}
