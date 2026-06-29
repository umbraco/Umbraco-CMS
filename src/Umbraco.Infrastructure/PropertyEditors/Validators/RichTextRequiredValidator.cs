using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

internal sealed class RichTextRequiredValidator : RequiredValidator, IRichTextRequiredValidator
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextRequiredValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextRequiredValidator"/> class, used to validate that rich text fields are not empty.
    /// </summary>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="logger">The <see cref="ILogger{RichTextRequiredValidator}"/> instance used for logging validation events.</param>
    public RichTextRequiredValidator(IJsonSerializer jsonSerializer, ILogger<RichTextRequiredValidator> logger) : base()
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Validates that the specified rich text value is present and meets the required criteria.
    /// </summary>
    /// <param name="value">The value to validate, typically representing the content of a rich text editor.</param>
    /// <param name="valueType">The type or format of the value being validated, if applicable.</param>
    /// <returns>An <see cref="IEnumerable{ValidationResult}"/> containing any validation errors related to the required rich text value.</returns>
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
