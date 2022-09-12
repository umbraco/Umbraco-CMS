using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class CultureImpactFactory : ICultureImpactFactory
{
    private ContentSettings _contentSettings;

    public CultureImpactFactory(IOptionsMonitor<ContentSettings> contentSettings)
    {
        _contentSettings = contentSettings.CurrentValue;

        contentSettings.OnChange(x => _contentSettings = x);
    }

    [Obsolete("Use constructor that takes IOptionsMonitor<SecuritySettings> instead. Scheduled for removal in V12")]
    public CultureImpactFactory(IOptionsMonitor<SecuritySettings> securitySettings)
        : this(StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ContentSettings>>())
    {
    }

    /// <inheritdoc/>
    public CultureImpact? Create(string? culture, bool isDefault, IContent content)
    {
        TryCreate(culture, isDefault, content.ContentType.Variations, true, _contentSettings.AllowEditInvariantFromNonDefault, out CultureImpact? impact);

        return impact;
    }

    /// <inheritdoc/>
    public CultureImpact ImpactAll() => CultureImpact.All;

    /// <inheritdoc/>
    public CultureImpact ImpactInvariant() => CultureImpact.Invariant;

    /// <inheritdoc/>
    public CultureImpact ImpactExplicit(string? culture, bool isDefault)
    {
        if (culture is null)
        {
            throw new ArgumentException("Culture <null> is not explicit.");
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture \"\" is not explicit.");
        }

        if (culture == "*")
        {
            throw new ArgumentException("Culture \"*\" is not explicit.");
        }

        return new CultureImpact(culture, isDefault, _contentSettings.AllowEditInvariantFromNonDefault);
    }

    /// <inheritdoc/>
    public string? GetCultureForInvariantErrors(IContent? content, string?[] savingCultures, string? defaultCulture)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (savingCultures is null)
        {
            throw new ArgumentNullException(nameof(savingCultures));
        }

        if (savingCultures.Length == 0)
        {
            throw new ArgumentException(nameof(savingCultures));
        }

        var cultureForInvariantErrors = savingCultures.Any(x => x.InvariantEquals(defaultCulture))
            // The default culture is being flagged for saving so use it
            ? defaultCulture
            // If the content has no published version, we need to affiliate validation with the first variant being saved.
            // If the content has a published version we will not affiliate the validation with any culture (null)
            : !content.Published ? savingCultures[0] : null;

        return cultureForInvariantErrors;
    }

    /// <summary>
    /// Tries to create an impact instance representing the impact of a culture set,
    /// in the context of a content item variation.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDefault">A value indicating whether the culture is the default culture.</param>
    /// <param name="variation">A content variation.</param>
    /// <param name="throwOnFail">A value indicating whether to throw if the impact cannot be created.</param>
    /// <param name="editInvariantFromNonDefault">A value indicating if publishing invariant properties from non-default language.</param>
    /// <param name="impact">The impact if it could be created, otherwise null.</param>
    /// <returns>A value indicating whether the impact could be created.</returns>
    /// <remarks>
    /// <para>Validates that the culture is compatible with the variation.</para>
    /// </remarks>
    internal bool TryCreate(string? culture, bool isDefault, ContentVariation variation, bool throwOnFail, bool editInvariantFromNonDefault, out CultureImpact? impact)
    {
        impact = null;

        // if culture is invariant...
        if (culture is null)
        {
            // ... then variation must not vary by culture ...
            if (variation.VariesByCulture())
            {
                if (throwOnFail)
                {
                    throw new InvalidOperationException("The invariant culture is not compatible with a varying variation.");
                }

                return false;
            }

            // ... and it cannot be default
            if (isDefault)
            {
                if (throwOnFail)
                {
                    throw new InvalidOperationException("The invariant culture can not be the default culture.");
                }

                return false;
            }

            impact = ImpactInvariant();
            return true;
        }

        // if culture is 'all'...
        if (culture == "*")
        {
            // ... it cannot be default
            if (isDefault)
            {
                if (throwOnFail)
                    throw new InvalidOperationException("The 'all' culture can not be the default culture.");
                return false;
            }

            // if variation does not vary by culture, then impact is invariant
            impact = variation.VariesByCulture() ? ImpactAll() : ImpactInvariant();
            return true;
        }

        // neither null nor "*" - cannot be the empty string
        if (culture.IsNullOrWhiteSpace())
        {
            if (throwOnFail)
            {
                throw new ArgumentException("Cannot be the empty string.", nameof(culture));
            }

            return false;
        }

        // if culture is specific, then variation must vary
        if (variation.VariesByCulture() is false)
        {
            if (throwOnFail)
            {
                throw new InvalidOperationException($"The variant culture {culture} is not compatible with an invariant variation.");
            }

            return false;
        }

        // return specific impact
        impact = new CultureImpact(culture, isDefault, editInvariantFromNonDefault);
        return true;
    }
}
