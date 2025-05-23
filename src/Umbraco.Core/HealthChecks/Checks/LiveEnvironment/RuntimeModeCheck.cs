// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.LiveEnvironment;

/// <summary>
///     Health check for the recommended production configuration for the runtime mode.
/// </summary>
[HealthCheck(
    "8E31E5C9-7A1D-4ACB-A3A8-6495F3EDB932",
    "Runtime Mode",
    Description = "The Production Runtime Mode disables development features and checks that settings are configured optimally for production.",
    Group = "Live Environment")]
public class RuntimeModeCheck : AbstractSettingsCheck
{
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeModeCheck" /> class.
    /// </summary>
    public RuntimeModeCheck(ILocalizedTextService textService, IOptionsMonitor<RuntimeSettings> runtimeSettings)
        : base(textService) =>
        _runtimeSettings = runtimeSettings;

    /// <inheritdoc />
    public override string ItemPath => Constants.Configuration.ConfigRuntimeMode;

    /// <inheritdoc />
    public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

    /// <inheritdoc />
    public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
    {
        new() { IsRecommended = true, Value = RuntimeMode.Production.ToString() },
    };

    /// <inheritdoc />
    public override string CurrentValue => _runtimeSettings.CurrentValue.Mode.ToString();

    /// <inheritdoc />
    public override string CheckSuccessMessage => LocalizedTextService.Localize("healthcheck", "runtimeModeCheckSuccessMessage");

    /// <inheritdoc />
    public override string CheckErrorMessage => LocalizedTextService.Localize("healthcheck", "runtimeModeCheckErrorMessage");

    /// <inheritdoc />
    public override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.LiveEnvironment.RuntimeModeCheck;
}
