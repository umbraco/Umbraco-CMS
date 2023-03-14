using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether a fixed Umbraco application URL is set when in production runtime mode.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators.RuntimeModeProductionValidatorBase" />
public class UmbracoApplicationUrlValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoApplicationUrlValidator" /> class.
    /// </summary>
    /// <param name="webRoutingSettings">The web routing settings.</param>
    public UmbracoApplicationUrlValidator(IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
        => _webRoutingSettings = webRoutingSettings;

    /// <inheritdoc />
    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(_webRoutingSettings.CurrentValue.UmbracoApplicationUrl))
        {
            validationErrorMessage = "Umbraco application URL needs to be set in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
