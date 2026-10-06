// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="ImagingSettings" />.
/// </summary>
/// <remarks>
///     Only the memory settings are validated. The rest predate this validator and are left as they
///     were rather than risk failing the boot of a site that has been running happily on them.
/// </remarks>
public class ImagingSettingsValidator : ConfigurationValidatorBase, IValidateOptions<ImagingSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ImagingSettings options)
    {
        ImagingMemorySettings memory = options.Memory;

        ValidateOptionsResult? failure =
            ValidateNotNegative(nameof(ImagingMemorySettings.MaximumPoolSizeMegabytes), memory.MaximumPoolSizeMegabytes)
            ?? ValidateNotNegative(nameof(ImagingMemorySettings.MaximumConcurrentProcessing), memory.MaximumConcurrentProcessing)
            ?? ValidateNotNegative(nameof(ImagingMemorySettings.MaximumDecodedImageMegabytes), memory.MaximumDecodedImageMegabytes);

        if (failure is not null)
        {
            return failure;
        }

        // No equivalent ceiling is imposed on the other two: a large value there is a legitimate, if
        // roundabout, way of asking for no bound, and neither costs anything to set up. Only the
        // pool size is sized against by the imaging library up front.
        if (memory.MaximumPoolSizeMegabytes > ImagingMemorySettings.MaximumConfigurablePoolSizeMegabytes)
        {
            return ValidateOptionsResult.Fail(
                $"Configuration entry {MemorySettingPath(nameof(ImagingMemorySettings.MaximumPoolSizeMegabytes))} must be " +
                $"{ImagingMemorySettings.MaximumConfigurablePoolSizeMegabytes} or less. Note that it is given in megabytes, not bytes.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult? ValidateNotNegative(string setting, int value)
        => value < 0
            ? ValidateOptionsResult.Fail(
                $"Configuration entry {MemorySettingPath(setting)} cannot be negative. " +
                "Use zero to derive a value from the memory available to the process.")
            : null;

    private static string MemorySettingPath(string setting)
        => $"{Constants.Configuration.ConfigImaging}:Memory:{setting}";
}
