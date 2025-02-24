using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents an integer property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Integer,
    ValueType = ValueTypes.Integer,
    ValueEditorIsReusable = true)]
public class IntegerPropertyEditor : DataEditor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerPropertyEditor"/> class.
    /// </summary>
    public IntegerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<IntegerPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new IntegerConfigurationEditor();

    /// <summary>
    /// Defines the value editor for the integer property editor.
    /// </summary>
    internal class IntegerPropertyValueEditor : DataValueEditor
    {
        private const string ConfigurationKeyMinValue = "min";
        private const string ConfigurationKeyMaxValue = "max";
        private const string ConfigurationKeyStepValue = "step";

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerPropertyValueEditor"/> class.
        /// </summary>
        public IntegerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => Validators.AddRange([new IntegerValidator(), new MinMaxValidator(), new StepValidator()]);

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
            => TryParsePropertyValue(property.GetValue(culture, segment));

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => TryParsePropertyValue(editorValue.Value);

        private int? TryParsePropertyValue(object? value)
            => value is int integerValue
                ? integerValue
                : int.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out var parsedIntegerValue)
                    ? parsedIntegerValue
                    : null;

        /// <summary>
        /// Base validator for configuration specific integer property editor validation.
        /// </summary>
        internal abstract class IntegerPropertyConfigurationValidatorBase : SimplePropertyConfigurationValidatorBase<int>
        {
            /// <inheritdoc/>
            protected override bool TryParsePropertyValue(object? value, out int parsedIntegerValue)
                => int.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out parsedIntegerValue);
        }

        /// <summary>
        /// Validates the min/max configuration for the integer property editor.
        /// </summary>
        internal class MinMaxValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
        {
            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (TryParsePropertyValue(value, out var parsedIntegerValue) is false)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMinValue, out int min) && parsedIntegerValue < min)
                {
                    yield return new ValidationResult($"The value {value} is less than the allowed minimum value of {min}", ["value"]);
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMaxValue, out int max) && parsedIntegerValue > max)
                {
                    yield return new ValidationResult($"The value {value} is greater than the allowed maximum value of {max}", ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the integer property editor.
        /// </summary>
        internal class StepValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
        {
            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (TryParsePropertyValue(value, out var parsedIntegerValue) is false)
                {
                    yield break;
                }

                if (dataTypeConfiguration is not Dictionary<string, object> configuration)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(configuration, ConfigurationKeyMinValue, out int min) &&
                    TryGetConfiguredValue(configuration, ConfigurationKeyStepValue, out int step) &&
                    IsValidForStep(parsedIntegerValue, min, step) is false)
                {
                    yield return new ValidationResult($"The value {value} does not correspond with the configured step value of {step}", ["value"]);
                }
            }

            private static bool IsValidForStep(int value, int min, int step)
            {
                if (value < min)
                {
                    return true; // Outside of the range, so we expect another validator will have picked this up.
                }

                return (value - min) % step == 0;
            }
        }
    }
}
