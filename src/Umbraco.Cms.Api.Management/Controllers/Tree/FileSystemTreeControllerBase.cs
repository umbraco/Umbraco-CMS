using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    protected abstract IFileSystem FileSystem { get; }

    protected abstract string FileIcon(string path);

    protected abstract string ItemType(string path);

    protected async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetRoot(int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        FileSystemTreeItemPresentationModel[] viewModels = GetPathViewModels(string.Empty, pageNumber, pageSize, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> GetChildren(string path, int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        FileSystemTreeItemPresentationModel[] viewModels = GetPathViewModels(path, pageNumber, pageSize, out var totalItems);

        PagedViewModel<FileSystemTreeItemPresentationModel> result = PagedViewModel(viewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> GetItems(string[] paths)
    {
        FileSystemTreeItemPresentationModel[] viewModels = paths
            .Where(FileSystem.FileExists)
            .Select(path =>
            {
                var fileName = GetFileName(path);
                return fileName.IsNullOrWhiteSpace()
                    ? null
                    : MapViewModel(path, fileName, false);
            }).WhereNotNull().ToArray();

        return await Task.FromResult(Ok(viewModels));
    }

    protected virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    protected virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .OrderBy(file => file)
        .ToArray();

    protected virtual string GetFileName(string path) => FileSystem.GetFileName(path);

    protected virtual bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    private FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, long pageNumber, int pageSize, out long totalItems)
    {
        var allItems = GetDirectories(path)
            .Select(directory => new { Path = directory, IsFolder = true })
            .Union(GetFiles(path).Select(file => new { Path = file, IsFolder = false }))
            .ToArray();

        totalItems = allItems.Length;

        FileSystemTreeItemPresentationModel ViewModel(string itemPath, bool isFolder)
            => MapViewModel(
                itemPath,
                isFolder ? Path.GetFileName(itemPath) : FileSystem.GetFileName(itemPath),
                isFolder);

        return allItems
            .Skip((int)(pageNumber * pageSize))
            .Take(pageSize)
            .Select(item => ViewModel(item.Path, item.IsFolder))
            .ToArray();
    }

    private PagedViewModel<FileSystemTreeItemPresentationModel> PagedViewModel(IEnumerable<FileSystemTreeItemPresentationModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };

    private FileSystemTreeItemPresentationModel MapViewModel(string path, string name, bool isFolder)
        => new()
        {
            Path = path,
            Name = name,
            Icon = isFolder ? Constants.Icons.Folder : FileIcon(path),
            HasChildren = isFolder && DirectoryHasChildren(path),
            Type = ItemType(path),
            IsFolder = isFolder
        };
}
