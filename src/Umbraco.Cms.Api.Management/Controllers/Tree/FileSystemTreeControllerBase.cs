using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService;

    [Obsolete("Has been moved to the individual services. Scheduled to be removed in Umbraco 19")]
    protected abstract IFileSystem FileSystem { get; }

    protected FileSystemTreeControllerBase(IFileSystemTreeService fileSystemTreeService) => _fileSystemTreeService = fileSystemTreeService;

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
        FileSystemTreeItemPresentationModel[] viewModels = _fileSystemTreeService.GetSiblingsViewModel(path, before, after, out var totalBefore, out var totalAfter);

        SubsetViewModel<FileSystemTreeItemPresentationModel> result = new() { TotalBefore = totalBefore, TotalAfter = totalAfter, Items = viewModels };
        return Task.FromResult<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>>(Ok(result));
    }

    protected Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> GetAncestors(string path, bool includeSelf = true)
    {
        path = path.VirtualPathToSystemPath();
        FileSystemTreeItemPresentationModel[] models = _fileSystemTreeService.GetAncestorModels(path, includeSelf);

        return Task.FromResult<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>>(Ok(models));
    }

    private PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };


}
