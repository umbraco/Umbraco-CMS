using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether HTTPS is enforced when in production runtime mode.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators.RuntimeModeProductionValidatorBase" />
public class UseHttpsValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UseHttpsValidator" /> class.
    /// </summary>
    /// <param name="globalSettings">The global settings.</param>
    public UseHttpsValidator(IOptionsMonitor<GlobalSettings> globalSettings)
        => _globalSettings = globalSettings;

    /// <inheritdoc />
    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (!_globalSettings.CurrentValue.UseHttps)
        {
            validationErrorMessage = "Using HTTPS should be enforced in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
