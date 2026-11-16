using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
/// Provides common functionality to validators of numeric property values against a data type's configured
/// validation range and step.
/// </summary>
/// <typeparam name="TValue">The numeric type the property value and the step are parsed to.</typeparam>
/// <typeparam name="TConfiguration">The typed configuration of the data type.</typeparam>
/// <remarks>
/// The data type configuration arrives either as the typed configuration object or as the raw dictionary, so both
/// are read.
/// </remarks>
internal abstract class NumericPropertyConfigurationValidatorBase<TValue, TConfiguration> : SimplePropertyConfigurationValidatorBase<TValue>
    where TValue : struct
    where TConfiguration : class
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
    /// Initializes a new instance of the <see cref="NumericPropertyConfigurationValidatorBase{TValue, TConfiguration}"/> class.
    /// </summary>
    /// <param name="localizedTextService">The localized text service.</param>
    protected NumericPropertyConfigurationValidatorBase(ILocalizedTextService localizedTextService) => LocalizedTextService = localizedTextService;

    /// <summary>
    /// Gets the <see cref="ILocalizedTextService"/>.
    /// </summary>
    protected ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    /// Gets the validation range from the typed configuration.
    /// </summary>
    /// <param name="configuration">The typed configuration.</param>
    /// <returns>The configured validation range.</returns>
    protected abstract object? GetValidationRange(TConfiguration configuration);

    /// <summary>
    /// Gets the step from the typed configuration.
    /// </summary>
    /// <param name="configuration">The typed configuration.</param>
    /// <returns>The configured step, or <c>null</c> when not configured.</returns>
    protected abstract TValue? GetStep(TConfiguration configuration);

    /// <summary>
    /// Retrieves the configured validation range from either the typed or the dictionary configuration.
    /// </summary>
    /// <param name="dataTypeConfiguration">The data type configuration.</param>
    /// <param name="min">The minimum bound, or <c>null</c> when unbounded or not present.</param>
    /// <param name="max">The maximum bound, or <c>null</c> when unbounded or not present.</param>
    /// <returns>True if a validation range was found.</returns>
    protected bool TryGetRange(object? dataTypeConfiguration, out decimal? min, out decimal? max)
        => dataTypeConfiguration is TConfiguration configuration
            ? RangeConfigurationHelper.TryGetBounds(GetValidationRange(configuration), out min, out max)
            : TryGetConfiguredRange(dataTypeConfiguration, ConfigurationKeyValidationRange, out min, out max);

    /// <summary>
    /// Retrieves the configured step from either the typed or the dictionary configuration.
    /// </summary>
    /// <param name="dataTypeConfiguration">The data type configuration.</param>
    /// <param name="step">The configured step.</param>
    /// <returns>True if a step was configured.</returns>
    protected bool TryGetStep(object? dataTypeConfiguration, out TValue step)
    {
        if (dataTypeConfiguration is TConfiguration configuration)
        {
            TValue? configuredStep = GetStep(configuration);
            step = configuredStep ?? default;
            return configuredStep.HasValue;
        }

        return TryGetConfiguredValue(dataTypeConfiguration, ConfigurationKeyStepValue, out step);
    }
}
