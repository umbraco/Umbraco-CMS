using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Extensions;

public class UmbracoStaticFilePathGenerator : IStaticFilePathGenerator
{
    private string? _backofficePath;
    private string? _backofficeAssetsPath;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IRuntimeMinifier _runtimeMinifier;
    private readonly IPackageManifestService _packageManifestService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IOptions<GlobalSettings> _globalSettings;

    public UmbracoStaticFilePathGenerator(
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        IRuntimeMinifier runtimeMinifier,
        IPackageManifestService packageManifestService,
        IJsonSerializer jsonSerializer,
        IOptions<GlobalSettings> globalSettings)
    {
        _hostingEnvironment = hostingEnvironment;
        _umbracoVersion = umbracoVersion;
        _runtimeMinifier = runtimeMinifier;
        _packageManifestService = packageManifestService;
        _jsonSerializer = jsonSerializer;
        _globalSettings = globalSettings;
    }

    /// <summary>
    ///     Gets the virtual path for the Backoffice through the GlobalSettings.
    /// </summary>
    public string BackofficePath => _backofficePath ??= _globalSettings.Value.GetBackOfficePath(_hostingEnvironment);

    /// <summary>
    ///     Gets the virtual path for the Backoffice assets coming from the Umbraco.Cms.StaticAssets RCL.
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
                _backofficeAssetsPath = BackofficePath.EnsureEndsWith('/') + "backoffice/" + umbracoHash;
            }

            return _backofficeAssetsPath;
        }
    }

    /// <inheritdoc cref="IStaticFilePathGenerator.GetBackofficePackageExportsAsync"/>
    public async Task<string> GetBackofficePackageExportsAsync()
    {
        PackageManifestImportmap packageImports = await _packageManifestService.GetPackageManifestImportmapAsync();

        var sb = new StringBuilder();
        sb.AppendLine(@"{ ""imports"": ");
        sb.AppendLine(_jsonSerializer.Serialize(packageImports.Imports));

        if (packageImports.Scopes is null == false && packageImports.Scopes.Count != 0)
        {
            sb.AppendLine(@", ""scopes"": ");
            sb.AppendLine(_jsonSerializer.Serialize(packageImports.Scopes));
        }

        sb.AppendLine(@"}");

        var importString = sb.ToString();

        // Inject the BackOffice cache buster into the import string
        return importString.Replace("/umbraco/backoffice", BackofficeAssetsPath);
    }
}
