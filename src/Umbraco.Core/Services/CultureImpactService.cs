using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class CultureImpactService : ICultureImpactService
{
    private SecuritySettings _securitySettings;

    public CultureImpactService(IOptionsMonitor<SecuritySettings> securitySettings)
    {
        _securitySettings = securitySettings.CurrentValue;

        securitySettings.OnChange(x => _securitySettings = x);
    }

    /// <inheritdoc/>
    public CultureImpact? Create(string culture, bool isDefault, IContent content)
        => CultureImpact.Create(culture, isDefault, content, _securitySettings.AllowEditInvariantFromNonDefault);

    public CultureImpact CreateImpactAll() => CultureImpact.All;

    public CultureImpact CreateInvariant() => CultureImpact.Invariant;

    public CultureImpact CreateExplicit(string? culture, bool isDefault)
        => CultureImpact.Explicit(culture, isDefault, _securitySettings.AllowEditInvariantFromNonDefault);

    public string? GetCultureForInvariantErrors(IContent? content, string?[] savingCultures, string? defaultCulture)
        => CultureImpact.GetCultureForInvariantErrors(content, savingCultures, defaultCulture);
}
