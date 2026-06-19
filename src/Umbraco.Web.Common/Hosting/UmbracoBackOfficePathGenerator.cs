using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Hosting;

/// <inheritdoc />
public class UmbracoBackOfficePathGenerator : IBackOfficePathGenerator
{
    private string? _backofficeAssetsPath;
    private string? _backOfficeVirtualDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoBackOfficePathGenerator"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public UmbracoBackOfficePathGenerator(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion)
        : this(
            hostingEnvironment,
            umbracoVersion,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<RuntimeSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoBackOfficePathGenerator"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <param name="umbracoVersion">The Umbraco version.</param>
    /// <param name="runtimeSettings">Runtime settings, used to fold the optional cache-bust seed into the back office cache-bust hash.</param>
    [ActivatorUtilitiesConstructor]
    public UmbracoBackOfficePathGenerator(
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        IOptions<RuntimeSettings> runtimeSettings)
    {
        BackOfficePath = hostingEnvironment.GetBackOfficePath();
        BackOfficeCacheBustHash = CacheBustHashGenerator.Generate(hostingEnvironment, umbracoVersion, runtimeSettings.Value.CacheBuster);
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
