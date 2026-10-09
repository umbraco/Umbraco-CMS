// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Core.Configuration.Models.Validation;

/// <summary>
///     Validator for configuration represented as <see cref="ScheduledPublishingSettings" />.
/// </summary>
public class ScheduledPublishingSettingsValidator : ConfigurationValidatorBase, IValidateOptions<ScheduledPublishingSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ScheduledPublishingSettings options)
    {
        if (options.Period <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"Configuration entry {Constants.Configuration.ConfigScheduledPublishing}:Period must be greater than zero.");
        }

        if (options.AlignToClock && IsCleanDivisorOfAnHour(options.Period) == false)
        {
            return ValidateOptionsResult.Fail(
                $"Configuration entry {Constants.Configuration.ConfigScheduledPublishing}:Period must be a whole number of seconds that divides evenly into one hour (3600 seconds) when {Constants.Configuration.ConfigScheduledPublishing}:AlignToClock is enabled, e.g. 10, 12, 15, 20, 30 or 60 seconds.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool IsCleanDivisorOfAnHour(TimeSpan period)
    {
        var totalSeconds = period.TotalSeconds;

        // Must be a positive, whole number of seconds (no sub-second component).
        if (totalSeconds <= 0 || totalSeconds != Math.Floor(totalSeconds))
        {
            return false;
        }

        return 3600 % (long)totalSeconds == 0;
    }
}
