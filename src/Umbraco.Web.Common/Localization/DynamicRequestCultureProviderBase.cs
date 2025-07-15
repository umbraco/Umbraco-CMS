using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace Umbraco.Cms.Web.Common.Localization;

/// <summary>
/// Base implementation that dynamically adds the determined cultures to the supported cultures.
/// </summary>
public abstract class DynamicRequestCultureProviderBase : RequestCultureProvider
{
    private readonly RequestLocalizationOptions _options;
    private readonly Lock _lockerSupportedCultures = new();
    private readonly Lock _lockerSupportedUICultures = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRequestCultureProviderBase" /> class.
    /// </summary>
    /// <param name="requestLocalizationOptions">The request localization options.</param>
    protected DynamicRequestCultureProviderBase(RequestLocalizationOptions requestLocalizationOptions)
        => _options = Options = requestLocalizationOptions;

    /// <inheritdoc />
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ProviderCultureResult? result = GetProviderCultureResult(httpContext);
        if (result is not null)
        {
            // We need to dynamically change the supported cultures since we won't ever know what languages are used since
            // they are dynamic within Umbraco. We have to handle this for both UI and Region cultures, in case people run different region and UI languages
            // This code to check existence is borrowed from aspnetcore to avoid creating a CultureInfo
            // https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Middleware/Localization/src/RequestLocalizationMiddleware.cs#L165
            TryAddLocked(_options.SupportedCultures, result.Cultures, _lockerSupportedCultures, (culture) =>
            {
                _options.SupportedCultures ??= new List<CultureInfo>();
                _options.SupportedCultures.Add(CultureInfo.GetCultureInfo(culture.ToString()));
            });

            TryAddLocked(_options.SupportedUICultures, result.UICultures, _lockerSupportedUICultures, (culture) =>
            {
                _options.SupportedUICultures ??= new List<CultureInfo>();
                _options.SupportedUICultures.Add(CultureInfo.GetCultureInfo(culture.ToString()));
            });

            return Task.FromResult<ProviderCultureResult?>(result);
        }

        return NullProviderCultureResult;
    }

    /// <summary>
    /// Gets the provider culture result.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>
    /// The provider culture result.
    /// </returns>
    protected abstract ProviderCultureResult? GetProviderCultureResult(HttpContext httpContext);

    /// <summary>
    /// Executes the <paramref name="addAction" /> within a double-checked lock when the a culture in <paramref name="cultures" /> does not exist in <paramref name="supportedCultures" />.
    /// </summary>
    /// <param name="supportedCultures">The supported cultures.</param>
    /// <param name="cultures">The cultures.</param>
    /// <param name="locker">The locker object to use.</param>
    /// <param name="addAction">The add action to execute.</param>
    private static void TryAddLocked(IEnumerable<CultureInfo>? supportedCultures, IEnumerable<StringSegment> cultures, Lock locker, Action<StringSegment> addAction)
    {
        foreach (StringSegment culture in cultures)
        {
            Func<CultureInfo, bool> predicate = x => culture.Equals(x.Name, StringComparison.OrdinalIgnoreCase);
            if (supportedCultures?.Any(predicate) is not true)
            {
                lock (locker)
                {
                    if (supportedCultures?.Any(predicate) is not true)
                    {
                        addAction(culture);
                    }
                }
            }
        }
    }
}
