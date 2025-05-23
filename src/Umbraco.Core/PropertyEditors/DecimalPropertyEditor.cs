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
    Constants.PropertyEditors.Aliases.Decimal,
    ValueType = ValueTypes.Decimal,
    ValueEditorIsReusable = true)]
public class DecimalPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalPropertyEditor" /> class.
    /// </summary>
    public DecimalPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<DecimalPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new DecimalConfigurationEditor();


    /// <summary>
    /// Defines the value editor for the decimal property editor.
    /// </summary>
    internal class DecimalPropertyValueEditor : DataValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalPropertyValueEditor"/> class.
        /// </summary>
        public DecimalPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => Validators.AddRange([new DecimalValidator(), new MinMaxValidator(localizedTextService), new StepValidator(localizedTextService)]);

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
            => TryParsePropertyValue(property.GetValue(culture, segment));

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => TryParsePropertyValue(editorValue.Value);

        private decimal? TryParsePropertyValue(object? value)
            => value is decimal decimalValue
                ? decimalValue
                : decimal.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out var parsedDecimalValue)
                    ? parsedDecimalValue
                    : null;

        /// <summary>
        /// Base validator for the decimal property editor validation against data type configured values.
        /// </summary>
        internal abstract class DecimalPropertyConfigurationValidatorBase : SimplePropertyConfigurationValidatorBase<double>
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
            /// Initializes a new instance of the <see cref="DecimalPropertyConfigurationValidatorBase"/> class.
            /// </summary>
            protected DecimalPropertyConfigurationValidatorBase(ILocalizedTextService localizedTextService) => LocalizedTextService = localizedTextService;

            /// <summary>
            /// Gets the <see cref="ILocalizedTextService"/>.
            /// </summary>
            protected ILocalizedTextService LocalizedTextService { get; }

            /// <inheritdoc/>
            protected override bool TryParsePropertyValue(object? value, out double parsedDecimalValue)
                => double.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out parsedDecimalValue);
        }

        /// <summary>
        /// Validates the min/max configuration for the decimal property editor.
        /// </summary>
        internal class MinMaxValidator : DecimalPropertyConfigurationValidatorBase, IValueValidator
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
                if (TryParsePropertyValue(value, out var parsedDecimalValue) is false)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMinValue, out double min) && parsedDecimalValue < min)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMinimum", [parsedDecimalValue.ToString(), min.ToString()]),
                        ["value"]);
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMaxValue, out double max) && parsedDecimalValue > max)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMaximum", [parsedDecimalValue.ToString(), max.ToString()]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the decimal property editor.
        /// </summary>
        internal class StepValidator : DecimalPropertyConfigurationValidatorBase, IValueValidator
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
                if (TryParsePropertyValue(value, out var parsedDecimalValue) is false)
                {
                    yield break;
                }

                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyMinValue, out double min) &&
                    TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyStepValue, out double step) &&
                    ValidationHelper.IsValueValidForStep((decimal)parsedDecimalValue, (decimal)min, (decimal)step) is false)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidStep", [parsedDecimalValue.ToString(), step.ToString(), min.ToString()]),
                        ["value"]);
                }
            }
        }
    }
}
