using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Serves as the base controller for file system tree management in the Umbraco CMS API, providing shared functionality for file system tree operations.
/// </summary>
public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemTreeControllerBase"/> class.
    /// </summary>
    /// <param name="fileSystemTreeService">The service that reads the file system tree.</param>
    protected FileSystemTreeControllerBase(IFileSystemTreeService fileSystemTreeService) => _fileSystemTreeService = fileSystemTreeService;

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
        FileSystemTreeItemPresentationModel[] models = _fileSystemTreeService.GetAncestorModels(path, includeSelf);

        return Task.FromResult<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>>(Ok(models));
    }

    private FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems)
        => _fileSystemTreeService.GetPathViewModels(path, skip, take, out totalItems);

    private PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };
}
