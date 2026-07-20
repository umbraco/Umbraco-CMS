using System.Reflection;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Templates.PartialViews;

/// <inheritdoc />
internal sealed class PartialViewPopulator : IPartialViewPopulator
{
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewPopulator"/> class with the specified file service.
    /// </summary>
    /// <param name="fileService">The <see cref="IFileService"/> used to perform file operations for partial views.</param>
    public PartialViewPopulator(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Gets the core assembly containing the Constants type.
    /// </summary>
    /// <returns>The core assembly.</returns>
    public Assembly GetCoreAssembly() => typeof(Constants).Assembly;

    /// <summary>
    /// Gets the base path to the embedded resources containing core partial views in Umbraco.
    /// </summary>
    public string CoreEmbeddedPath => "Umbraco.Cms.Core.EmbeddedResources";

    /// <inheritdoc/>
    public void CopyPartialViewIfNotExists(Assembly assembly, string embeddedPath, string fileSystemPath)
    {
        Stream? content = assembly.GetManifestResourceStream(embeddedPath);
        if (content is not null)
        {

            // We have to ensure that this is idempotent, so only save the view if it does not already exist
            // We don't want to overwrite any changes made.
            IPartialView? existingView = _fileService.GetPartialView(fileSystemPath);
            if (existingView is null)
            {
                var view = new PartialView(fileSystemPath)
                {
                    Content = GetTextFromStream(content)
                };

                _fileService.SavePartialView(view);
            }
        }
    }

    private string GetTextFromStream(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}
