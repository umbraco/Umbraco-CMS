// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates that the value against a regular expression.
/// </summary>
public sealed class RegexValidator : IValueFormatValidator, IManifestValueValidator
{
    private const string ValueIsInvalid = "Value is invalid, it does not match the correct pattern";
    private readonly ILocalizedTextService _textService;
    private string _regex;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegexValidator" /> class.
    /// </summary>
    /// <remarks>
    ///     Use this constructor when the validator is used as an <see cref="IValueFormatValidator" />,
    ///     and the regular expression is supplied at validation time. This constructor is also used when
    ///     the validator is used as an <see cref="IManifestValueValidator" /> and the regular expression
    ///     is supplied via the <see cref="Configuration" /> method.
    /// </remarks>
    public RegexValidator(ILocalizedTextService textService)
        : this(textService, string.Empty)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegexValidator" /> class.
    /// </summary>
    /// <remarks>
    ///     Use this constructor when the validator is used as an <see cref="IValueValidator" />,
    ///     and the regular expression must be supplied when the validator is created.
    /// </remarks>
    public RegexValidator(ILocalizedTextService textService, string regex)
    {
        _textService = textService;
        _regex = regex;
    }

    /// <summary>
    ///     Gets or sets the configuration, when parsed as <see cref="IManifestValueValidator" />.
    /// </summary>
    public string Configuration
    {
        get => _regex;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Value can't be empty or consist only of white-space characters.",
                    nameof(value));
            }

            _regex = value;
        }
    }

    /// <inheritdoc cref="IManifestValueValidator.ValidationName" />
    public string ValidationName => "Regex";

    /// <inheritdoc cref="IValueValidator.Validate" />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
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
            yield return new ValidationResult(
                _textService?.Localize("validation", "invalidPattern") ?? ValueIsInvalid,
                new[] { "value" });
        }
    }
}
