using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
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
public class DecimalPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalPropertyEditor" /> class.
    /// </summary>
    public DecimalPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalPropertyEditor" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 21.")]
    public DecimalPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : this(dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IIOHelper>())
    {
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(decimal?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("number", "null"),
        };

        // Add min/max/step constraints from configuration if available. The configuration arrives either as the
        // typed configuration object (from the data type's ConfigurationObject) or as the raw dictionary.
        if (configuration is DecimalConfiguration or IDictionary<string, object>)
        {
            object? rangeValue = configuration switch
            {
                DecimalConfiguration decimalConfiguration => decimalConfiguration.ValidationRange,
                IDictionary<string, object> configDict when configDict.TryGetValue("validationRange", out var range) => range,
                _ => null,
            };

            if (RangeConfigurationHelper.TryGetBounds(rangeValue, out decimal? min, out decimal? max))
            {
                if (min.HasValue)
                {
                    schema["minimum"] = (double)min.Value;
                }

                if (max.HasValue)
                {
                    schema["maximum"] = (double)max.Value;
                }
            }

            var step = configuration switch
            {
                DecimalConfiguration decimalConfiguration when decimalConfiguration.Step.HasValue => (double)decimalConfiguration.Step.Value,
                IDictionary<string, object> configDict when configDict.TryGetValue("step", out var stepValue) && stepValue is double stepDouble => stepDouble,
                _ => (double?)null,
            };

            // Default: allow up to 6 decimal places, matching DB DECIMAL(38,6).
            schema["multipleOf"] = step is double configuredStep && configuredStep > 0 ? configuredStep : 0.000001;
        }

        return schema;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<DecimalPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new DecimalConfigurationEditor(_ioHelper);

    /// <summary>
    /// Defines the value editor for the decimal property editor.
    /// </summary>
    internal sealed class DecimalPropertyValueEditor : DataValueEditor
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

        private static decimal? TryParsePropertyValue(object? value)
            => value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                IFormattable f => decimal.TryParse(f.ToString(null, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDecimalValue)
                        ? parsedDecimalValue
                        : null,
                _ => decimal.TryParse(value?.ToString(), CultureInfo.CurrentCulture, out var parsedDecimalValue)
                        ? parsedDecimalValue
                        : null,
            };

        /// <summary>
        /// Base validator for the decimal property editor validation against data type configured values.
        /// </summary>
        internal abstract class DecimalPropertyConfigurationValidatorBase : SimplePropertyConfigurationValidatorBase<double>
        {
            /// <summary>
            /// The configuration key for the validation range.
            /// </summary>
            protected const string ConfigurationKeyValidationRange = "validationRange";

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
            {
                if (value is null)
                {
                    parsedDecimalValue = default;
                    return false;
                }

                if (value is decimal decimalValue)
                {
                    parsedDecimalValue = (double)decimalValue;
                    return true;
                }

                if (value is double doubleValue)
                {
                    parsedDecimalValue = doubleValue;
                    return true;
                }

                if (value is float floatValue)
                {
                    parsedDecimalValue = (double)floatValue;
                    return true;
                }

                if (value is IFormattable formattableValue)
                {
                    return double.TryParse(formattableValue.ToString(null, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDecimalValue);
                }

                return double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsedDecimalValue);
            }
        }

        /// <summary>
        /// Validates the min/max configuration for the decimal property editor.
        /// </summary>
        internal sealed class MinMaxValidator : DecimalPropertyConfigurationValidatorBase, IValueValidator
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

                if (TryGetConfiguredRange(dataTypeConfiguration, ConfigurationKeyValidationRange, out decimal? min, out decimal? max) is false)
                {
                    yield break;
                }

                if (min.HasValue && parsedDecimalValue < (double)min.Value)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMinimum", [parsedDecimalValue.ToString(), min.Value.ToString(CultureInfo.InvariantCulture)]),
                        ["value"]);
                }

                if (max.HasValue && parsedDecimalValue > (double)max.Value)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMaximum", [parsedDecimalValue.ToString(), max.Value.ToString(CultureInfo.InvariantCulture)]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the decimal property editor.
        /// </summary>
        internal sealed class StepValidator : DecimalPropertyConfigurationValidatorBase, IValueValidator
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

                // Default min to 0 if not configured (step validation is relative to min).
                TryGetConfiguredRange(dataTypeConfiguration, ConfigurationKeyValidationRange, out decimal? configuredMin, out _);
                var min = configuredMin.HasValue ? (double)configuredMin.Value : 0d;

                // Default step to 0.000001 (6 decimal places) if not configured,
                // matching the database DECIMAL(38,6) column precision.
                if (TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyStepValue, out double step) is false)
                {
                    step = 0.000001;
                }

                if (ValidationHelper.IsValueValidForStep((decimal)parsedDecimalValue, (decimal)min, (decimal)step) is false)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidStep", [parsedDecimalValue.ToString(), step.ToString(), min.ToString()]),
                        ["value"]);
                }
            }
        }
    }
}
