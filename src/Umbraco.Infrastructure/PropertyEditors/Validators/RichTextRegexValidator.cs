using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal sealed class RichTextRegexValidator : IRichTextRegexValidator
{
    private readonly RegexValidator _regexValidator;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRegexValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.Validators.RichTextRegexValidator"/> class, used to validate rich text content using regular expressions.
    /// </summary>
    /// <param name="jsonSerializer">The serializer used for handling JSON data within the validator.</param>
    /// <param name="logger">The logger used for logging validation events and errors.</param>
    public RichTextRegexValidator(
        IJsonSerializer jsonSerializer,
        ILogger<RichTextRegexValidator> logger)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _regexValidator = new RegexValidator();
    }

    /// <summary>
    /// Validates whether the specified value matches the provided regular expression format.
    /// </summary>
    /// <param name="value">The value to be validated, typically extracted from a rich text editor.</param>
    /// <param name="valueType">The type or data format of the value, if applicable.</param>
    /// <param name="format">A regular expression pattern that the value should conform to.</param>
    /// <returns>An enumerable collection of <see cref="ValidationResult"/> objects indicating validation errors, or empty if the value is valid.</returns>
    public IEnumerable<ValidationResult> ValidateFormat(object? value, string? valueType, string format) => _regexValidator.ValidateFormat(GetValue(value), valueType, format);

    private object? GetValue(object? value) =>
        RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue)
            ? richTextEditorValue?.Markup
            : value;
}
