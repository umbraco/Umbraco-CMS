// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Configuration;

/// <summary>
///     Health check for the recommended production configuration for Macro Errors.
/// </summary>
[HealthCheck(
    "D0F7599E-9B2A-4D9E-9883-81C7EDC5616F",
    "Macro errors",
    Description = "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
    Group = "Configuration")]
public class MacroErrorsCheck : AbstractSettingsCheck
{
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly ILocalizedTextService _textService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroErrorsCheck" /> class.
    /// </summary>
    public MacroErrorsCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<ContentSettings> contentSettings)
        : base(textService)
    {
        _textService = textService;
        _contentSettings = contentSettings;
    }

    /// <inheritdoc />
    public override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.Configuration.MacroErrorsCheck;

    /// <inheritdoc />
    public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

    /// <inheritdoc />
    public override string ItemPath => Constants.Configuration.ConfigContentMacroErrors;

    /// <summary>
    ///     Gets the values to compare against.
    /// </summary>
    public override IEnumerable<AcceptableConfiguration> Values
    {
        get
        {
            var values = new List<AcceptableConfiguration>
            {
                new() { IsRecommended = true, Value = MacroErrorBehaviour.Inline.ToString() },
                new() { IsRecommended = false, Value = MacroErrorBehaviour.Silent.ToString() },
            };

            return values;
        }
    }

    /// <inheritdoc />
    public override string CurrentValue => _contentSettings.CurrentValue.MacroErrors.ToString();

    /// <summary>
    ///     Gets the message for when the check has succeeded.
    /// </summary>
    public override string CheckSuccessMessage =>
        _textService.Localize(
            "healthcheck", "macroErrorModeCheckSuccessMessage", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });

    /// <summary>
    ///     Gets the message for when the check has failed.
    /// </summary>
    public override string CheckErrorMessage =>
        _textService.Localize(
            "healthcheck", "macroErrorModeCheckErrorMessage", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
}
