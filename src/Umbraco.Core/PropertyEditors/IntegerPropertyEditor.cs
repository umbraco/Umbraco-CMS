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
    Constants.PropertyEditors.Aliases.Integer,
    ValueType = ValueTypes.Integer,
    ValueEditorIsReusable = true)]
public class IntegerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerPropertyEditor"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 21.")]
    public IntegerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : this(dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IIOHelper>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerPropertyEditor"/> class.
    /// </summary>
    public IntegerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(int?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration)
    {
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = new JsonArray("integer", "null"),
        };

        // Add min/max constraints from configuration if available. The configuration arrives either as the typed
        // configuration object (from the data type's ConfigurationObject) or as the raw dictionary.
        object? rangeValue = configuration switch
        {
            IntegerConfiguration integerConfiguration => integerConfiguration.ValidationRange,
            IDictionary<string, object> configDict when configDict.TryGetValue("validationRange", out var range) => range,
            _ => null,
        };

        if (RangeConfigurationHelper.TryGetBounds(rangeValue, out decimal? min, out decimal? max))
        {
            if (min.HasValue)
            {
                schema["minimum"] = (int)min.Value;
            }

            if (max.HasValue)
            {
                schema["maximum"] = (int)max.Value;
            }
        }

        var step = configuration switch
        {
            IntegerConfiguration integerConfiguration => integerConfiguration.Step,
            IDictionary<string, object> configDict when configDict.TryGetValue("step", out var stepValue) && stepValue is int stepInt => stepInt,
            _ => null,
        };

        if (step is int configuredStep && configuredStep > 1)
        {
            schema["multipleOf"] = configuredStep;
        }

        return schema;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<IntegerPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new IntegerConfigurationEditor(_ioHelper);

    /// <summary>
    /// Defines the value editor for the integer property editor.
    /// </summary>
    internal sealed class IntegerPropertyValueEditor : DataValueEditor
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
            /// The configuration key for the validation range.
            /// </summary>
            protected const string ConfigurationKeyValidationRange = "validationRange";

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
        internal sealed class MinMaxValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
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

                if (TryGetConfiguredRange(dataTypeConfiguration, ConfigurationKeyValidationRange, out decimal? min, out decimal? max) is false)
                {
                    yield break;
                }

                if (min.HasValue && parsedIntegerValue < min.Value)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMinimum", [parsedIntegerValue.ToString(), ((int)min.Value).ToString()]),
                        ["value"]);
                }

                if (max.HasValue && parsedIntegerValue > max.Value)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMaximum", [parsedIntegerValue.ToString(), ((int)max.Value).ToString()]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the integer property editor.
        /// </summary>
        internal sealed class StepValidator : IntegerPropertyConfigurationValidatorBase, IValueValidator
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

                TryGetConfiguredRange(dataTypeConfiguration, ConfigurationKeyValidationRange, out decimal? min, out _);

                if (min.HasValue &&
                    TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyStepValue, out int step) &&
                    ValidationHelper.IsValueValidForStep(parsedIntegerValue, (int)min.Value, step) is false)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidStep", [parsedIntegerValue.ToString(), step.ToString(), ((int)min.Value).ToString()]),
                        ["value"]);
                }
            }
        }
    }
}
