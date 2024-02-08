using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class FileItemPresentationFactory : IFileItemPresentationFactory
{
    private readonly FileSystems _fileSystems;
    private readonly IPhysicalFileSystem _physicalFileSystem;

    public FileItemPresentationFactory(FileSystems fileSystems, IPhysicalFileSystem physicalFileSystem)
    {
        _fileSystems = fileSystems;
        _physicalFileSystem = physicalFileSystem;
    }

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

    public IEnumerable<StaticFileItemResponseModel> CreateStaticFileItemResponseModels(IEnumerable<string> paths)
        => CreateItemResponseModels<StaticFileItemResponseModel>(
            paths,
            _physicalFileSystem,
            (name, path, parent, isFolder) => new() { Name = name, Path = path, Parent = parent, IsFolder = isFolder });

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
