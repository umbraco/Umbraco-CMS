// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value against a regular expression.
/// </summary>
public sealed class RegexValidator : IValueFormatValidator, IValueValidator
{
    private string _regex;

    [Obsolete($"Use the constructor that does not accept {nameof(ILocalizedTextService)}. Will be removed in V15.")]
    public RegexValidator(ILocalizedTextService textService)
        : this(string.Empty)
    {
    }

    [Obsolete($"Use the constructor that does not accept {nameof(ILocalizedTextService)}. Will be removed in V15.")]
    public RegexValidator(ILocalizedTextService textService, string regex)
        : this(regex)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegexValidator" /> class.
    /// </summary>
    /// <remarks>
    ///     Use this constructor when the validator is used as an <see cref="IValueFormatValidator" />,
    ///     and the regular expression is supplied at validation time.
    /// </remarks>
    public RegexValidator()
        : this(string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegexValidator" /> class.
    /// </summary>
    /// <remarks>
    ///     Use this constructor when the validator is used as an <see cref="IValueValidator" />,
    ///     and the regular expression must be supplied when the validator is created.
    /// </remarks>
    public RegexValidator(string regex)
        => _regex = regex;

    /// <inheritdoc cref="IValueValidator.Validate" />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
    {
        if (_regex == null)
        {
            throw new InvalidOperationException("The validator has not been configured.");
        }

        return ValidateFormat(value, valueType, _regex);
    }

    /// <inheritdoc cref="IValueFormatValidator.ValidateFormat" />
    public IEnumerable<ValidationResult> ValidateFormat(object? value, string? valueType, string format)
    {
        if (format == null)
        {
            throw new ArgumentNullException(nameof(format));
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(format));
        }

        if (value == null || !new Regex(format).IsMatch(value.ToString()!))
        {
            yield return new ValidationResult(Constants.Validation.ErrorMessages.Properties.PatternMismatch, new[] { "value" });
        }
    }
}
