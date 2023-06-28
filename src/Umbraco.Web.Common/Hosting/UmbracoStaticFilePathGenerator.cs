using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Extensions;

public class UmbracoStaticFilePathGenerator : IStaticFilePathGenerator
{
    private string? _backofficeAssetsPath;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IRuntimeMinifier _runtimeMinifier;
    private readonly IOptions<GlobalSettings> _globalSettings;

    public UmbracoStaticFilePathGenerator(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion, IRuntimeMinifier runtimeMinifier, IOptions<GlobalSettings> globalSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _umbracoVersion = umbracoVersion;
        _runtimeMinifier = runtimeMinifier;
        _globalSettings = globalSettings;
    }

    /// <summary>
    ///     Get the virtual path for the Backoffice assets coming from the Umbraco.Cms.StaticAssets RCL.
    ///     The path will contain a generated SHA1 hash that is based on a number of parameters including the UmbracoVersion and runtime minifier.
    /// </summary>
    /// <example>/umbraco/backoffice/addf120b430021c36c232c99ef8d926aea2acd6b</example>
    /// <see cref="UrlHelperExtensions.GetCacheBustHash"/>
    public string BackofficeAssetsPath
    {
        get
        {
            if (_backofficeAssetsPath is null)
            {
                var umbracoHash = UrlHelperExtensions.GetCacheBustHash(_hostingEnvironment, _umbracoVersion, _runtimeMinifier);
                var backOfficePath = _globalSettings.Value.GetBackOfficePath(_hostingEnvironment);
                _backofficeAssetsPath = backOfficePath.EnsureEndsWith('/') + "backoffice/" + umbracoHash;
            }

            return _backofficeAssetsPath;
        }
    }
}
