using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for file items within the management API.
/// </summary>
public class FileItemPresentationFactory : IFileItemPresentationFactory
{
    private readonly FileSystems _fileSystems;
    private readonly IPhysicalFileSystem _physicalFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileItemPresentationFactory"/> class, providing access to file system abstractions for file item presentation.
    /// </summary>
    /// <param name="fileSystems">An abstraction for managing multiple file systems within the application.</param>
    /// <param name="physicalFileSystem">An abstraction representing the physical file system used for file operations.</param>
    public FileItemPresentationFactory(FileSystems fileSystems, IPhysicalFileSystem physicalFileSystem)
    {
        _fileSystems = fileSystems;
        _physicalFileSystem = physicalFileSystem;
    }

    /// <summary>
    /// Creates a collection of <see cref="PartialViewItemResponseModel"/> instances for the specified file or folder paths.
    /// </summary>
    /// <param name="paths">A collection of file or folder paths to generate response models for.</param>
    /// <returns>An <see cref="IEnumerable{PartialViewItemResponseModel}"/> representing the items at the specified paths.</returns
    public IEnumerable<PartialViewItemResponseModel> CreatePartialViewItemResponseModels(IEnumerable<string> paths)
        => CreateItemResponseModels<PartialViewItemResponseModel>(
            paths,
            _fileSystems.PartialViewsFileSystem,
            (name, path, parent, isFolder) => new() { Name = name, Path = path, Parent = parent, IsFolder = isFolder });

    public IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths)
        => CreateItemResponseModels<ScriptItemResponseModel>(
            paths,
            _fileSystems.ScriptsFileSystem,
            (name, path, parent, isFolder) => new() { Name = name, Path = path, Parent = parent, IsFolder = isFolder });

    /// <summary>
    /// Creates a collection of <see cref="StaticFileItemResponseModel"/> instances from the given file paths.
    /// </summary>
    /// <param name="paths">The collection of file paths to create response models for.</param>
    /// <returns>An enumerable of <see cref="StaticFileItemResponseModel"/> representing the files.</returns>
    public IEnumerable<StaticFileItemResponseModel> CreateStaticFileItemResponseModels(IEnumerable<string> paths)
        => CreateItemResponseModels<StaticFileItemResponseModel>(
            paths,
            _physicalFileSystem,
            (name, path, parent, isFolder) => new() { Name = name, Path = path, Parent = parent, IsFolder = isFolder });

    /// <summary>
    /// Creates a collection of <see cref="Umbraco.Cms.Api.Management.Models.StylesheetItemResponseModel"/> instances from the given stylesheet paths.
    /// </summary>
    /// <param name="paths">The enumerable of stylesheet file paths to create response models for.</param>
    /// <returns>An enumerable of <see cref="Umbraco.Cms.Api.Management.Models.StylesheetItemResponseModel"/> representing the stylesheet items.</returns>
    public IEnumerable<StylesheetItemResponseModel> CreateStylesheetItemResponseModels(IEnumerable<string> paths)
        => CreateItemResponseModels<StylesheetItemResponseModel>(
            paths,
            _fileSystems.StylesheetsFileSystem,
            (name, path, parent, isFolder) => new() { Name = name, Path = path, Parent = parent, IsFolder = isFolder });

    private IEnumerable<T> CreateItemResponseModels<T>(IEnumerable<string> paths, IFileSystem? fileSystem, Func<string, string, FileSystemFolderModel?, bool, T> itemFactory)
        where T : FileSystemResponseModelBase
        => fileSystem != null
            ? paths.Where(path => fileSystem.DirectoryExists(path) || fileSystem.FileExists(path)).Select(path =>
                {
                    var fileName = fileSystem.GetFileName(path);
                    var parentPath = Path.GetDirectoryName(path);
                    var isFolder = fileSystem.GetExtension(path).IsNullOrWhiteSpace();
                    FileSystemFolderModel? parent = parentPath.IsNullOrWhiteSpace()
                        ? null
                        : new FileSystemFolderModel { Path = parentPath.SystemPathToVirtualPath() };
                    return itemFactory(fileName, path.SystemPathToVirtualPath(), parent, isFolder);
                })
                .ToArray()
            : throw new ArgumentException("Missing file system", nameof(fileSystem));
}
