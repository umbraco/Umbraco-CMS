using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Provides the default implementation of <see cref="IDefaultCultureAccessor" />.
/// </summary>
public class DefaultCultureAccessor : IDefaultCultureAccessor
{
    private readonly ILanguageService _languageService;
    private readonly IRuntimeState _runtimeState;
    private GlobalSettings _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultCultureAccessor" /> class.
    /// </summary>
    public DefaultCultureAccessor(ILanguageService languageService, IRuntimeState runtimeState, IOptionsMonitor<GlobalSettings> options)
    {
        _languageService = languageService;
        _runtimeState = runtimeState;
        _options = options.CurrentValue;
        options.OnChange(x => _options = x);
    }

    /// <inheritdoc />
    public string DefaultCulture => _runtimeState.Level == RuntimeLevel.Run
        ? _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult()
        : _options.DefaultUILanguage; // default for install and upgrade, when the service is n/a
}
