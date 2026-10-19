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
    /// <remarks>
    ///     <see cref="RuntimeLevel.Upgrading" /> is included because during a background unattended upgrade the
    ///     database is connected and the site is serving: reporting the configured fallback there left content
    ///     unroutable (https://github.com/umbraco/Umbraco-CMS/issues/22581).
    /// </remarks>
    public string DefaultCulture => _runtimeState.Level is RuntimeLevel.Run or RuntimeLevel.Upgrading
        ? _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult() ?? string.Empty
        : _options.DefaultUILanguage; // no database to read from yet, e.g. install or early boot
}
