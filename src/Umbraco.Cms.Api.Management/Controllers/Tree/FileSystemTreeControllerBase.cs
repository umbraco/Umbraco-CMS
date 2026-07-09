using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Serves as the base controller for file system tree management in the Umbraco CMS API, providing shared functionality for file system tree operations.
/// </summary>
public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService = null!;

    /// <summary>
    /// Indicates whether to use the IFileSystemTreeService or the legacy implementation.
    /// </summary>
    /// <remarks>
    /// This is retained to ensure that any controllers outside of the CMS that use this base class with the obsolete constructor
    /// continue to function until they can be updated to use the new service.
    /// To be removed along with the constructor taking no parameters in Umbraco 19.
    /// </remarks>
    private readonly bool _useFileSystemTreeService = true;

    [Obsolete("Has been moved to the individual services. Scheduled to be removed in Umbraco 19.")]
    protected abstract IFileSystem FileSystem { get; }

    [ActivatorUtilitiesConstructor]
    protected FileSystemTreeControllerBase(IFileSystemTreeService fileSystemTreeService) => _fileSystemTreeService = fileSystemTreeService;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    protected FileSystemTreeControllerBase() => _useFileSystemTreeService = false;

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

    /// <summary>
    /// Gets the siblings of the targeted item based on its path.
    /// </summary>
    /// <param name="path">The path to the item.</param>
    /// <param name="before">The amount of siblings you want to fetch from before the items position in the array.</param>
    /// <param name="after">The amount of siblings you want to fetch after the items position in the array.</param>
    /// <returns>A SubsetViewModel of the siblings of the item and the item itself.</returns>
    protected Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> GetSiblings(string path, int before, int after)
    {
        FileSystemTreeItemPresentationModel[] viewModels = _fileSystemTreeService.GetSiblingsViewModels(path, before, after, out var totalBefore, out var totalAfter);

        SubsetViewModel<FileSystemTreeItemPresentationModel> result = new() { TotalBefore = totalBefore, TotalAfter = totalAfter, Items = viewModels };
        return Task.FromResult<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    protected virtual Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> GetAncestors(string path, bool includeSelf = true)
    {
        path = path.VirtualPathToSystemPath();
        FileSystemTreeItemPresentationModel[] models = GetAncestorModels(path, includeSelf);

        return Task.FromResult<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>>(Ok(models));
    }

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
    protected virtual FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf)
    {
        if (_useFileSystemTreeService)
        {
            return _fileSystemTreeService.GetAncestorModels(path, includeSelf);
        }

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

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
    protected virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
    protected virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .OrderBy(file => file)
        .ToArray();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
    protected virtual bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
    private string GetFileSystemItemName(bool isFolder, string itemPath) => isFolder
        ? Path.GetFileName(itemPath)
        : FileSystem.GetFileName(itemPath);

    private FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems)
    {
        if (_useFileSystemTreeService)
        {
            return _fileSystemTreeService.GetPathViewModels(path, skip, take, out totalItems);
        }

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

    private PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 19.")]
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
