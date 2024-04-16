using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Hosting;

/// <inheritdoc />
public class UmbracoBackOfficePathGenerator : IBackOfficePathGenerator
{
    private string? _backofficeAssetsPath;
    private string? _backOfficeVirtualDirectory;

    public UmbracoBackOfficePathGenerator(
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        IOptions<GlobalSettings> globalSettings)
    {
        BackOfficePath = globalSettings.Value.GetBackOfficePath(hostingEnvironment);
        BackOfficeCacheBustHash = UrlHelperExtensions.GetCacheBustHash(hostingEnvironment, umbracoVersion);
    }

    /// <inheritdoc />
    public string BackOfficePath { get; }

    /// <inheritdoc />
    public string BackOfficeCacheBustHash { get; }

    /// <inheritdoc />
    public string BackOfficeVirtualDirectory => _backOfficeVirtualDirectory ??= BackOfficePath.EnsureEndsWith('/') + "backoffice";

    /// <summary>
    ///     Gets the virtual path for the Backoffice assets coming from the Umbraco.Cms.StaticAssets RCL.
    ///     The path will contain a generated SHA1 hash that is based on a number of parameters including the UmbracoVersion and runtime minifier.
    /// </summary>
    /// <example>/umbraco/backoffice/addf120b430021c36c232c99ef8d926aea2acd6b</example>
    /// <see cref="UrlHelperExtensions.GetCacheBustHash"/>
    public string BackOfficeAssetsPath =>
        _backofficeAssetsPath ??= BackOfficeVirtualDirectory.EnsureEndsWith('/') + BackOfficeCacheBustHash;
}
