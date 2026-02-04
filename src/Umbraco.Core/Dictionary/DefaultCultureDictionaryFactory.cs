using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Dictionary;

/// <summary>
///     A culture dictionary factory used to create an Umbraco.Core.Dictionary.ICultureDictionary.
/// </summary>
/// <remarks>
///     In the future this will allow use to potentially store dictionary items elsewhere and allows for maximum
///     flexibility.
/// </remarks>
public class DefaultCultureDictionaryFactory : ICultureDictionaryFactory
{
    private readonly AppCaches _appCaches;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultCultureDictionaryFactory"/> class.
    /// </summary>
    /// <param name="localizationService">The localization service for accessing dictionary items.</param>
    /// <param name="appCaches">The application caches containing the request cache.</param>
    public DefaultCultureDictionaryFactory(ILocalizationService localizationService, AppCaches appCaches)
    {
        _localizationService = localizationService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public ICultureDictionary CreateDictionary() =>
        new DefaultCultureDictionary(_localizationService, _appCaches.RequestCache);

    /// <inheritdoc />
    public ICultureDictionary CreateDictionary(CultureInfo specificCulture) =>
        new DefaultCultureDictionary(specificCulture, _localizationService, _appCaches.RequestCache);
}
