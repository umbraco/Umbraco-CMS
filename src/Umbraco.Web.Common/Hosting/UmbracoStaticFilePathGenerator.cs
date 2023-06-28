using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.WebAssets;
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

    public string BackofficeAssetsPath
    {
        get
        {
            if (_backofficeAssetsPath is null)
            {
                var umbracoHash = UrlHelperExtensions.GetCacheBustHash(_hostingEnvironment, _umbracoVersion, _runtimeMinifier);
                var backOfficePath = _globalSettings.Value.GetBackOfficePath(_hostingEnvironment);
                _backofficeAssetsPath =  backOfficePath.EnsureEndsWith('/') + "backoffice/" + umbracoHash;
            }

            return _backofficeAssetsPath;
        }
    }
}
