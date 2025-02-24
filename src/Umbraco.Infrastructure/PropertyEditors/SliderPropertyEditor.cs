// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Nodes;
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
/// Represents a slider editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Slider,
    ValueEditorIsReusable = true)]
public class SliderPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SliderPropertyEditor" /> class.
    /// </summary>
    public SliderPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<SliderPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SliderConfigurationEditor(_ioHelper);

    /// <summary>
    /// Defines the value editor for the slider property editor.
    /// </summary>
    internal class SliderPropertyValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderPropertyValueEditor"/> class.
        /// </summary>
        public SliderPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            ILocalizedTextService localizedTextService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
            Validators.AddRange(new RangeValidator(localizedTextService), new MinMaxValidator(localizedTextService), new StepValidator(localizedTextService));
        }

        /// <inheritdoc/>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // value is stored as a string - either a single decimal value
            // or a two decimal values separated by comma (for range sliders)
            var value = property.GetValue(culture, segment);
            if (value is not string stringValue)
            {
                return null;
            }

            var parts = stringValue.Split(Constants.CharArrays.Comma);
            var parsed = parts
                .Select(s => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var i) ? i : (decimal?)null)
                .Where(i => i != null)
                .Select(i => i!.Value)
                .ToArray();

            return parts.Length == parsed.Length && parsed.Length is 1 or 2
                ? new SliderRange { From = parsed.First(), To = parsed.Last() }
                : null;
        }

        /// <inheritdoc/>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is not null && _jsonSerializer.TryDeserialize(editorValue.Value, out SliderRange? sliderRange)
                ? sliderRange.ToString()
                : null;

        /// <summary>
        /// Represents a slide value.
        /// </summary>
        internal class SliderRange
        {
            /// <summary>
            /// Gets or sets the slider range from value.
            /// </summary>
            public decimal From { get; set; }

            /// <summary>
            /// Gets or sets the slide range to value.
            /// </summary>
            public decimal To { get; set; }

            /// <inheritdoc/>
            public override string ToString()
                => From == To
                    ? $"{From.ToString(CultureInfo.InvariantCulture)}"
                    : $"{From.ToString(CultureInfo.InvariantCulture)},{To.ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Base validator for configuration specific integer property editor validation.
        /// </summary>
        internal abstract class SliderPropertyConfigurationValidatorBase
        {
            /// <summary>
            /// The configuration key for the minimum value.
            /// </summary>
            protected const string ConfigurationKeyMinValue = "minVal";

            /// <summary>
            /// The configuration key for the maximum value.
            /// </summary>
            protected const string ConfigurationKeyMaxValue = "maxVal";

            /// <summary>
            /// The configuration key for the step value.
            /// </summary>
            protected const string ConfigurationKeyStepValue = "step";

            /// <summary>
            /// The configuration key for the enable range value.
            /// </summary>
            protected const string ConfigurationKeyEnableRangeValue = "enableRange";

            /// <summary>
            /// Initializes a new instance of the <see cref="SliderPropertyConfigurationValidatorBase"/> class.
            /// </summary>
            protected SliderPropertyConfigurationValidatorBase(ILocalizedTextService localizedTextService) => LocalizedTextService = localizedTextService;

            /// <summary>
            /// Gets the <see cref="ILocalizedTextService"/>.
            /// </summary>
            protected ILocalizedTextService LocalizedTextService { get; }

            /// <summary>
            /// Parses a <see cref="SliderRange"/> from the provided value.
            /// </summary>
            protected bool TryParsePropertyValue(object? value, [NotNullWhen(true)] out SliderRange? parsedValue)
            {
                if (value is null || value is not JsonObject valueAsJsonObject)
                {
                    parsedValue = null;
                    return false;
                }

                var from = GetDecimalValue(valueAsJsonObject, nameof(SliderRange.From).ToLowerInvariant());
                var to = GetDecimalValue(valueAsJsonObject, nameof(SliderRange.To).ToLowerInvariant());
                if (from.HasValue is false || to.HasValue is false)
                {
                    parsedValue = null;
                    return false;
                }

                parsedValue = new SliderRange
                {
                    From = from.Value,
                    To = to.Value,
                };

                return true;
            }

            private static decimal? GetDecimalValue(JsonObject valueAsJsonObject, string key)
                => valueAsJsonObject[key]?.GetValue<decimal>();
        }

        /// <summary>
        /// Validates the range configuration for the slider property editor.
        /// </summary>
        internal class RangeValidator : SliderPropertyConfigurationValidatorBase, IValueValidator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MinMaxValidator"/> class.
            /// </summary>
            public RangeValidator(ILocalizedTextService localizedTextService)
                : base(localizedTextService)
            {
            }

            /// <inheritdoc/>
            public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration, PropertyValidationContext validationContext)
            {
                if (TryParsePropertyValue(value, out SliderRange? sliderRange) is false)
                {
                    yield break;
                }

                if (dataTypeConfiguration is not SliderConfiguration sliderConfiguration)
                {
                    yield break;
                }

                if (sliderConfiguration.EnableRange is false && sliderRange.From != sliderRange.To)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "unexpectedRange", [sliderRange.ToString()]),
                        ["value"]);
                }

                if (sliderConfiguration.EnableRange && sliderRange.To < sliderRange.From)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidRange", [sliderRange.ToString()]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the min/max configuration for the slider property editor.
        /// </summary>
        internal class MinMaxValidator : SliderPropertyConfigurationValidatorBase, IValueValidator
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
                if (TryParsePropertyValue(value, out SliderRange? sliderRange) is false)
                {
                    yield break;
                }

                if (dataTypeConfiguration is not SliderConfiguration sliderConfiguration)
                {
                    yield break;
                }

                if (sliderRange.From < sliderConfiguration.MinimumValue)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMinimum", [sliderRange.From.ToString(), sliderConfiguration.MinimumValue.ToString()]),
                        ["value"]);
                }

                if (sliderRange.To > sliderConfiguration.MaximumValue)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "outOfRangeMaximum", [sliderRange.To.ToString(), sliderConfiguration.MaximumValue.ToString()]),
                        ["value"]);
                }
            }
        }

        /// <summary>
        /// Validates the step configuration for the slider property editor.
        /// </summary>
        internal class StepValidator : SliderPropertyConfigurationValidatorBase, IValueValidator
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
                if (TryParsePropertyValue(value, out SliderRange? sliderRange) is false)
                {
                    yield break;
                }

                if (dataTypeConfiguration is not SliderConfiguration sliderConfiguration)
                {
                    yield break;
                }

                if (IsValidForStep(sliderRange.From, sliderConfiguration.MinimumValue, sliderConfiguration.Step) is false ||
                    IsValidForStep(sliderRange.To, sliderConfiguration.MinimumValue, sliderConfiguration.Step) is false)
                {
                    yield return new ValidationResult(
                        LocalizedTextService.Localize("validation", "invalidStep", [sliderRange.ToString(), sliderConfiguration.Step.ToString(), sliderConfiguration.MinimumValue.ToString()]),
                        ["value"]);
                }
            }

            private static bool IsValidForStep(decimal value, decimal min, decimal step)
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
