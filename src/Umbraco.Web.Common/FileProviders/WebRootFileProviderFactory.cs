using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Web.Common.FileProviders;

public class WebRootFileProviderFactory : IManifestFileProviderFactory, IGridEditorsConfigFileProviderFactory
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebRootFileProviderFactory"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The web hosting environment an application is running in.</param>
    public WebRootFileProviderFactory(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    ///     Creates a new <see cref="IFileProvider" /> instance, pointing at <see cref="IWebHostEnvironment.WebRootPath"/>.
    /// </summary>
    /// <returns>
    ///     The newly created <see cref="IFileProvider" /> instance.
    /// </returns>
    public IFileProvider Create() => _webHostEnvironment.WebRootFileProvider;
}
