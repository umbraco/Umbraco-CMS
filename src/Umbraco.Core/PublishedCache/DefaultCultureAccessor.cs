using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Provides the default implementation of <see cref="IDefaultCultureAccessor" />.
/// </summary>
public class DefaultCultureAccessor : IDefaultCultureAccessor
{
    private readonly ILanguageService _languageService;
    private readonly IRuntimeState _runtimeState;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Lock _lock = new();
    private GlobalSettings _options;
    private volatile string? _defaultIsoCode;
    private int _version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultCultureAccessor" /> class.
    /// </summary>
    public DefaultCultureAccessor(
        ILanguageService languageService,
        IRuntimeState runtimeState,
        ICoreScopeProvider scopeProvider,
        IOptionsMonitor<GlobalSettings> options)
    {
        _runtimeState = runtimeState;
        _scopeProvider = scopeProvider;
        _options = options.CurrentValue;
        options.OnChange(x => _options = x);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultCultureAccessor" /> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 20.")]
    public DefaultCultureAccessor(ILanguageService languageService, IRuntimeState runtimeState, IOptionsMonitor<GlobalSettings> options)
        : this(languageService, runtimeState, StaticServiceProvider.Instance.GetRequiredService<ICoreScopeProvider>(), options)
    {
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <see cref="RuntimeLevel.Upgrading" /> is included because during a background unattended upgrade the
    ///     database is connected and the site is serving: reporting the configured fallback there left content
    ///     unroutable (https://github.com/umbraco/Umbraco-CMS/issues/22581).
    /// </remarks>
    public string DefaultCulture => _runtimeState.Level is RuntimeLevel.Run or RuntimeLevel.Upgrading
        ? GetDefaultIsoCode()
        : _options.DefaultUILanguage; // no database to read from yet, e.g. install or early boot

    /// <inheritdoc />
    public void ClearCache()
    {
        lock (_lock)
        {
            _version++;
            _defaultIsoCode = null;
        }
    }

    private string GetDefaultIsoCode()
    {
        var defaultIsoCode = _defaultIsoCode;
        if (defaultIsoCode is not null)
        {
            return defaultIsoCode;
        }

        int version;
        lock (_lock)
        {
            version = _version;
        }

        defaultIsoCode = _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult() ?? string.Empty;

        // Only a value that can be trusted to last is cached:
        // - an empty one means no default language has been set up yet, which a running site always has;
        // - while upgrading, migrations may still change the languages without a cache refresh;
        // - within an ambient scope the value may not be committed yet and could be rolled back.
        // A clear while reading means the value may be outdated already.
        if (defaultIsoCode.Length > 0
            && _runtimeState.Level is RuntimeLevel.Run
            && _scopeProvider.Context is null)
        {
            lock (_lock)
            {
                if (version == _version)
                {
                    _defaultIsoCode = defaultIsoCode;
                }
            }
        }

        return defaultIsoCode;
    }
}
