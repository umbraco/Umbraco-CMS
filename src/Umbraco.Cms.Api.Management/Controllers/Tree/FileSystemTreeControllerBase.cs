using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    protected abstract IFileSystem FileSystem { get; }

    protected Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetRoot(int skip, int take)
    {
        FileSystemTreeItemPresentationModel[] viewModels = GetPathViewModels(string.Empty, skip, take, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    protected Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetChildren(string path, int skip, int take)
    {
        FileSystemTreeItemPresentationModel[] viewModels = GetPathViewModels(path, skip, take, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    protected virtual Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> GetAncestors(string path, bool includeSelf = true)
    {
        path = path.VirtualPathToSystemPath();
        FileSystemTreeItemPresentationModel[] models = GetAncestorModels(path, includeSelf);

        return Task.FromResult<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>>(Ok(models));
    }

    protected virtual FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf)
    {
        var directories = path.Split(Path.DirectorySeparatorChar).Take(Range.EndAt(Index.FromEnd(1))).ToArray();
        var result = directories
            .Select((directory, index) => MapViewModel(string.Join(Path.DirectorySeparatorChar, directories.Take(index + 1)), directory, true))
            .ToList();

        if (includeSelf)
        {
            var selfIsFolder = FileSystem.FileExists(path) is false;
            result.Add(MapViewModel(path, GetFileSystemItemName(selfIsFolder, path), selfIsFolder));
        }

        return result.ToArray();
    }

    protected virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    protected virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .OrderBy(file => file)
        .ToArray();

    protected virtual bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    private FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems)
    {
        path = path.VirtualPathToSystemPath();
        var allItems = GetDirectories(path)
            .Select(directory => new { Path = directory, IsFolder = true })
            .Union(GetFiles(path).Select(file => new { Path = file, IsFolder = false }))
            .ToArray();

        totalItems = allItems.Length;

        FileSystemTreeItemPresentationModel ViewModel(string itemPath, bool isFolder)
            => MapViewModel(
                itemPath,
                GetFileSystemItemName(isFolder, itemPath),
                isFolder);

        return allItems
            .Skip(skip)
            .Take(take)
            .Select(item => ViewModel(item.Path, item.IsFolder))
            .ToArray();
    }

    private string GetFileSystemItemName(bool isFolder, string itemPath) => isFolder
        ? Path.GetFileName(itemPath)
        : FileSystem.GetFileName(itemPath);

    private static PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };

    private FileSystemTreeItemPresentationModel MapViewModel(string path, string name, bool isFolder)
    {
        var parentPath = Path.GetDirectoryName(path);
        return new FileSystemTreeItemPresentationModel
        {
            Path = path.SystemPathToVirtualPath(),
            Name = name,
            HasChildren = isFolder && DirectoryHasChildren(path),
            IsFolder = isFolder,
            Parent = parentPath.IsNullOrWhiteSpace()
                ? null
                : new FileSystemFolderModel { Path = parentPath.SystemPathToVirtualPath() }
        };
    }
}
