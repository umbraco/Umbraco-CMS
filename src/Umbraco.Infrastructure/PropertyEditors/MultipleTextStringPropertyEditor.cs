// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a multiple text string property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultipleTextstring,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class MultipleTextStringPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleTextStringPropertyEditor" /> class.
    /// </summary>
    public MultipleTextStringPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleTextStringPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultipleTextStringConfigurationEditor(_ioHelper);

    /// <summary>
    /// Defines the value editor for the multiple text string property editor.
    /// </summary>
    internal sealed class MultipleTextStringPropertyValueEditor : DataValueEditor
    {
        private static readonly string _newLine = "\n";
        private static readonly string[] _newLineDelimiters = { "\r\n", "\r", "\n" };

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleTextStringPropertyValueEditor"/> class.
        /// </summary>
        public MultipleTextStringPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            Validators.AddRange(new MinMaxValidator(localizedTextService));
        }

        /// <summary>
        /// A custom <see href="IValueFormatValidator" /> is used as for multiple text strings, each string should individually
        /// be checked against the configured regular expression, rather than the JSON representing all the strings as a whole.
        /// </summary>
        public override IValueFormatValidator FormatValidator => new MultipleTextStringFormatValidator();

        /// <summary>
        /// A custom <see cref="IValueRequiredValidator" /> is used to evaluate "required" against the count of non-empty
        /// strings, consistent with how <see cref="FromEditor"/> filters empty/whitespace strings before persistence.
        /// Without this, the default <see cref="RequiredValidator"/> would call <c>ToString()</c> on the array
        /// (yielding <c>"System.String[]"</c>) and incorrectly pass the required check.
        /// </summary>
        public override IValueRequiredValidator RequiredValidator => new MultipleTextStringRequiredValidator();

        /// <summary>
        /// The value passed in from the editor will be an array of simple objects so we'll need to parse them to get the
        /// string.
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// We will also check the pre-values here, if there are more items than what is allowed we'll just trim the end.
        /// </remarks>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is not IEnumerable<string> value)
            {
                return null;
            }

            var nonEmptyValues = value.Where(x => string.IsNullOrWhiteSpace(x) is false).ToArray();
            if (nonEmptyValues.Length == 0)
            {
                return null;
            }

            return string.Join(_newLine, nonEmptyValues);
        }

        /// <inheritdoc/>
        public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            // The legacy property editor saved this data as new line delimited! strange but we have to maintain that.
            return value is string stringValue
                ? SplitPropertyValue(stringValue)
                : Array.Empty<string>();
        }

        internal static string[] SplitPropertyValue(string propertyValue)
            => propertyValue.Split(_newLineDelimiters, StringSplitOptions.None);
    }

    /// <summary>
    /// A custom <see href="IValueFormatValidator" /> to check each string against the configured format.
    /// </summary>
    internal sealed class MultipleTextStringFormatValidator : IValueFormatValidator
    {
        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidateFormat(object? value, string valueType, string format)
        {
            if (value is not IEnumerable<string> textStrings)
            {
                return [];
            }

            var textStringValidator = new RegexValidator();
            foreach (var textString in textStrings)
            {
                if (string.IsNullOrWhiteSpace(textString))
                {
                    continue;
                }

                var validationResults = textStringValidator.ValidateFormat(textString, valueType, format).ToList();
                if (validationResults.Any())
                {
                    return validationResults;
                }
            }

            return [];
        }
    }

    /// <summary>
    /// A custom <see cref="IValueRequiredValidator" /> that treats an array of empty/whitespace-only strings as "no value",
    /// consistent with how <see cref="MultipleTextStringPropertyValueEditor.FromEditor"/> filters them before persistence.
    /// </summary>
    internal sealed class MultipleTextStringRequiredValidator : IValueRequiredValidator
    {
        private static readonly RequiredValidator _defaultValidator = new();

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidateRequired(object? value, string valueType)
        {
            if (value is IEnumerable<string> strings && strings.Any(s => string.IsNullOrWhiteSpace(s) is false) is false)
            {
                // All strings are empty/whitespace — treat as no value.
                return _defaultValidator.ValidateRequired(null, valueType);
            }

            return _defaultValidator.ValidateRequired(value, valueType);
        }
    }

    /// <summary>
    /// Validates the min/max configuration for the multiple text strings property editor.
    /// </summary>
    internal sealed class MinMaxValidator : IValueValidator
    {
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
        /// </summary>
        public MinMaxValidator(ILocalizedTextService localizedTextService) => _localizedTextService = localizedTextService;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
        {
            if (dataTypeConfiguration is not MultipleTextStringConfiguration multipleTextStringConfiguration)
            {
                yield break;
            }

            // Handle both a newline delimited string and an IEnumerable<string> as the value (see: https://github.com/umbraco/Umbraco-CMS/pull/18936).
            // If we have a null value, treat as a string count of zero for minimum number validation.
            var stringCount = value is string stringValue
                ? MultipleTextStringPropertyValueEditor.SplitPropertyValue(stringValue)
                    .Count(s => string.IsNullOrWhiteSpace(s) is false)
                : value is IEnumerable<string> strings
                    ? strings.Count(s => string.IsNullOrWhiteSpace(s) is false)
                    : 0;

            if (stringCount < multipleTextStringConfiguration.Min)
            {
                if (stringCount == 1)
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "outOfRangeSingleItemMinimum", [multipleTextStringConfiguration.Min.ToString()]),
                        ["value"]);
                }
                else
                {
                    yield return new ValidationResult(
                        _localizedTextService.Localize("validation", "outOfRangeMultipleItemsMinimum", [stringCount.ToString(), multipleTextStringConfiguration.Min.ToString()]),
                        ["value"]);
                }
            }

            if (multipleTextStringConfiguration.Max > 0 && stringCount > multipleTextStringConfiguration.Max)
            {
                yield return new ValidationResult(
                    _localizedTextService.Localize("validation", "outOfRangeMultipleItemsMaximum", [stringCount.ToString(), multipleTextStringConfiguration.Max.ToString()]),
                    ["value"]);
            }
        }
    }
}
