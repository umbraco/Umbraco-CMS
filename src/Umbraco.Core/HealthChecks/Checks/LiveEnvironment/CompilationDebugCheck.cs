// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.LiveEnvironment;

/// <summary>
///     Health check for the configuration of debug-flag.
/// </summary>
[HealthCheck(
    "61214FF3-FC57-4B31-B5CF-1D095C977D6D",
    "Debug Compilation Mode",
    Description = "Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.",
    Group = "Live Environment")]
public class CompilationDebugCheck : AbstractSettingsCheck
{
    private readonly IOptionsMonitor<HostingSettings> _hostingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompilationDebugCheck" /> class.
    /// </summary>
    public CompilationDebugCheck(ILocalizedTextService textService, IOptionsMonitor<HostingSettings> hostingSettings)
        : base(textService) =>
        _hostingSettings = hostingSettings;

    /// <inheritdoc />
    public override string ItemPath => Constants.Configuration.ConfigHostingDebug;

    /// <inheritdoc />
    public override string ReadMoreLink =>
        Constants.HealthChecks.DocumentationLinks.LiveEnvironment.CompilationDebugCheck;

    /// <inheritdoc />
    public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

    /// <inheritdoc />
    public override IEnumerable<AcceptableConfiguration> Values => new List<AcceptableConfiguration>
    {
        new() { IsRecommended = true, Value = bool.FalseString.ToLower() },
    };

    /// <inheritdoc />
    public override string CurrentValue => _hostingSettings.CurrentValue.Debug.ToString();

    /// <inheritdoc />
    public override string CheckSuccessMessage =>
        LocalizedTextService.Localize("healthcheck", "compilationDebugCheckSuccessMessage");

    /// <inheritdoc />
    public override string CheckErrorMessage =>
        LocalizedTextService.Localize("healthcheck", "compilationDebugCheckErrorMessage");
}
