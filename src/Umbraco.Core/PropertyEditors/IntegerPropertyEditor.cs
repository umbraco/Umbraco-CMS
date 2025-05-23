using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///  Represents a decimal property editor.
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
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerPropertyValueEditor"/> class.
        /// </summary>
        public IntegerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => Validators.AddRange([new IntegerValidator(), new MinMaxValidator(localizedTextService), new StepValidator(localizedTextService)]);

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
        /// /// Base validator for the integer property editor validation against data type configured values.
        /// </summary>
        internal abstract class IntegerPropertyConfigurationValidatorBase : SimplePropertyConfigurationValidatorBase<int>
        {
            /// <summary>
            /// The configuration key for the minimum value.
            /// </summary>
            protected const string ConfigurationKeyMinValue = "min";

            /// <summary>
            /// The configuration key for the maximum value.
            /// </summary>
            protected const string ConfigurationKeyMaxValue = "max";

            /// <summary>
            /// The configuration key for the step value.
            /// </summary>
            protected const string ConfigurationKeyStepValue = "step";

            /// <summary>
            /// Initializes a new instance of the <see cref="IntegerPropertyConfigurationValidatorBase"/> class.
            /// </summary>
            protected IntegerPropertyConfigurationValidatorBase(ILocalizedTextService localizedTextService) => LocalizedTextService = localizedTextService;

            /// <summary>
            /// Gets the <see cref="ILocalizedTextService"/>.
            /// </summary>
            protected ILocalizedTextService LocalizedTextService { get; }

            /// <inheritdoc/>
            protected override bool TryParsePropertyValue(object? value, out int parsedIntegerValue)
                => int.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out parsedIntegerValue);
        }

        /// <summary>
        /// Validates the min/max configuration for the integer property editor.
        /// </summary>
        internal class MinMaxValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
            /// </summary>
            public MinMaxValidator(ILocalizedTextService localizedTextService)
                : base(localizedTextService)
            {
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (TryParsePropertyValue(value, out var parsedIntegerValue) is false)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMinValue, out int min) && parsedIntegerValue < min)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMinimum", [parsedIntegerValue.ToString(), min.ToString()]),
                        ["value"]);
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMaxValue, out int max) && parsedIntegerValue > max)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMaximum", [parsedIntegerValue.ToString(), max.ToString()]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the integer property editor.
        /// </summary>
        internal class StepValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StepValidator"/> class.
            /// </summary>
            public StepValidator(ILocalizedTextService localizedTextService)
                : base(localizedTextService)
            {
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (TryParsePropertyValue(value, out var parsedIntegerValue) is false)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMinValue, out int min) &&
                    TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyStepValue, out int step) &&
                    ValidationHelper.IsValueValidForStep(parsedIntegerValue, min, step) is false)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidStep", [parsedIntegerValue.ToString(), step.ToString(), min.ToString()]),
                        ["value"]);
                }
            }
        }
    }
}
