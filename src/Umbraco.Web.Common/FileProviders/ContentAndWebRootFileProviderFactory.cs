using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Web.Common.FileProviders;

public class ContentAndWebRootFileProviderFactory : IPackageManifestFileProviderFactory
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentAndWebRootFileProviderFactory"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The web hosting environment an application is running in.</param>
    public ContentAndWebRootFileProviderFactory(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    ///     Creates a new <see cref="IFileProvider" /> instance, pointing at WebRootPath and ContentRootPath.
    /// </summary>
    /// <returns>
    ///     The newly created <see cref="IFileProvider" /> instance.
    /// </returns>
    public IFileProvider Create() => new CompositeFileProvider(_webHostEnvironment.WebRootFileProvider, _webHostEnvironment.ContentRootFileProvider);
}
