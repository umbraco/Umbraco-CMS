using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService;

    [Obsolete("Has been moved to the individual services. Scheduled to be removed in Umbraco 18.")]
    protected abstract IFileSystem FileSystem { get; }

    [ActivatorUtilitiesConstructor]
    protected FileSystemTreeControllerBase(IFileSystemTreeService fileSystemTreeService) => _fileSystemTreeService = fileSystemTreeService;

    [Obsolete("Use the other constructor. Scheduled for removal in Umbraco 18.")]
    protected FileSystemTreeControllerBase()
        : this(StaticServiceProvider.Instance.GetRequiredService<IScriptTreeService>())
    {
    }

    protected Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetRoot(int skip, int take)
    {
        FileSystemTreeItemPresentationModel[] viewModels = _fileSystemTreeService.GetPathViewModels(string.Empty, skip, take, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    protected Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetChildren(string path, int skip, int take)
    {
        FileSystemTreeItemPresentationModel[] viewModels = _fileSystemTreeService.GetPathViewModels(path, skip, take, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    /// <summary>
    /// Gets the sibling of the targeted item based on its path.
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
        FileSystemTreeItemPresentationModel[] models = _fileSystemTreeService.GetAncestorModels(path, includeSelf);

        return Task.FromResult<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>>(Ok(models));
    }

    private PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
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

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
    protected virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
    protected virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .OrderBy(file => file)
        .ToArray();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
    protected virtual bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
    private string GetFileSystemItemName(bool isFolder, string itemPath) => isFolder
        ? Path.GetFileName(itemPath)
        : FileSystem.GetFileName(itemPath);

    [Obsolete("Has been moved to FileSystemTreeServiceBase. Scheduled for removal in Umbraco 18.")]
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
