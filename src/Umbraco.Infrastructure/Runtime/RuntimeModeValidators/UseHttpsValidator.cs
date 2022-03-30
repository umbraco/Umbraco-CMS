using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

internal class UseHttpsValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

    public UseHttpsValidator(IOptionsMonitor<GlobalSettings> globalSettings)
        => _globalSettings = globalSettings;

    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Ensure HTTPS is enforced
        if (!_globalSettings.CurrentValue.UseHttps)
        {
            validationErrorMessage = "Using HTTPS should be enforced in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
