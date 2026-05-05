using System.Reflection;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.IO;

namespace Umbraco.Cms.Infrastructure.Templates.PartialViews;

/// <inheritdoc />
internal sealed class PartialViewPopulator : IPartialViewPopulator
{
    private readonly IPartialViewService _partialViewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewPopulator"/> class with the specified partial view service.
    /// </summary>
    /// <param name="partialViewService">The <see cref="IPartialViewService"/> used to perform file operations for partial views.</param>
    public PartialViewPopulator(IPartialViewService partialViewService)
    {
        _partialViewService = partialViewService;
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
        using Stream? resourceStream = assembly.GetManifestResourceStream(embeddedPath);
        if (resourceStream is null)
        {
            return;
        }

        // We have to ensure that this is idempotent, so only save the view if it does not already exist
        // We don't want to overwrite any changes made.
        IPartialView? existingView = _partialViewService.GetAsync(fileSystemPath).GetAwaiter().GetResult();
        if (existingView is not null)
        {
            return;
        }

        resourceStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(resourceStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        var content = reader.ReadToEnd();

        (var name, var parentPath) = ForwardSlashPath.Split(fileSystemPath);

        var createModel = new PartialViewCreateModel
        {
            Name = name,
            ParentPath = parentPath,
            Content = content,
        };

        _partialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
    }
}
