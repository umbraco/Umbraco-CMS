// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks;

/// <summary>
///     Provides a base class for health checks of configuration values.
/// </summary>
public abstract class AbstractSettingsCheck : HealthCheck
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AbstractSettingsCheck" /> class.
    /// </summary>
    protected AbstractSettingsCheck(ILocalizedTextService textService) => LocalizedTextService = textService;

    /// <summary>
    ///     Gets key within the JSON to check, in the colon-delimited format
    ///     https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1
    /// </summary>
    public abstract string ItemPath { get; }

    /// <summary>
    ///     Gets the localized text service.
    /// </summary>
    protected ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    ///     Gets a link to an external resource with more information.
    /// </summary>
    public abstract string ReadMoreLink { get; }

    /// <summary>
    ///     Gets the values to compare against.
    /// </summary>
    public abstract IEnumerable<AcceptableConfiguration> Values { get; }

    /// <summary>
    ///     Gets the current value of the config setting
    /// </summary>
    public abstract string CurrentValue { get; }

    /// <summary>
    ///     Gets the comparison type for checking the value.
    /// </summary>
    public abstract ValueComparisonType ValueComparisonType { get; }

    /// <summary>
    ///     Gets the message for when the check has succeeded.
    /// </summary>
    public virtual string CheckSuccessMessage => LocalizedTextService.Localize("healthcheck", "checkSuccessMessage", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath });

    /// <summary>
    ///     Gets the message for when the check has failed.
    /// </summary>
    public virtual string CheckErrorMessage =>
        ValueComparisonType == ValueComparisonType.ShouldEqual
            ? LocalizedTextService.Localize(
                "healthcheck", "checkErrorMessageDifferentExpectedValue", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath })
            : LocalizedTextService.Localize(
                "healthcheck", "checkErrorMessageUnexpectedValue", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath });

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatus()
    {
        // update the successMessage with the CurrentValue
        var successMessage = string.Format(CheckSuccessMessage, ItemPath, Values, CurrentValue);
        var valueFound = Values.Any(value =>
            string.Equals(CurrentValue, value.Value, StringComparison.InvariantCultureIgnoreCase));

        if ((ValueComparisonType == ValueComparisonType.ShouldEqual && valueFound)
            || (ValueComparisonType == ValueComparisonType.ShouldNotEqual && valueFound == false))
        {
            return Task.FromResult(
                new HealthCheckStatus(successMessage) { ResultType = StatusResultType.Success }.Yield());
        }

        var resultMessage = string.Format(CheckErrorMessage, ItemPath, Values, CurrentValue);
        var healthCheckStatus = new HealthCheckStatus(resultMessage)
        {
            ResultType = StatusResultType.Error,
            ReadMoreLink = ReadMoreLink,
        };

        return Task.FromResult(healthCheckStatus.Yield());
    }

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new NotSupportedException("Configuration cannot be automatically fixed.");
}
