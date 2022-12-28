using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Tree;

public abstract class FileSystemTreeControllerBase : ManagementApiControllerBase
{
    protected abstract IFileSystem FileSystem { get; }

    protected abstract string FileIcon(string path);

    protected abstract string ItemType(string path);

    protected async Task<ActionResult<PagedViewModel<FileSystemTreeItemViewModel>>> GetRoot(int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        FileSystemTreeItemViewModel[] viewModels = GetPathViewModels(string.Empty, pageNumber, pageSize, out var totalItems);

        PagedViewModel<FileSystemTreeItemViewModel> result = PagedViewModel(viewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedViewModel<FileSystemTreeItemViewModel>>> GetChildren(string path, int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        FileSystemTreeItemViewModel[] viewModels = GetPathViewModels(path, pageNumber, pageSize, out var totalItems);

        PagedViewModel<FileSystemTreeItemViewModel> result = PagedViewModel(viewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<IEnumerable<FileSystemTreeItemViewModel>>> GetItems(string[] paths)
    {
        FileSystemTreeItemViewModel[] viewModels = paths
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

    private FileSystemTreeItemViewModel[] GetPathViewModels(string path, long pageNumber, int pageSize, out long totalItems)
    {
        var allItems = GetDirectories(path)
            .Select(directory => new { Path = directory, IsFolder = true })
            .Union(GetFiles(path).Select(file => new { Path = file, IsFolder = false }))
            .ToArray();

        totalItems = allItems.Length;

        FileSystemTreeItemViewModel ViewModel(string itemPath, bool isFolder)
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

    private PagedViewModel<FileSystemTreeItemViewModel> PagedViewModel(IEnumerable<FileSystemTreeItemViewModel> viewModels, long totalItems)
        => new() { Total = totalItems, Items = viewModels };

    private FileSystemTreeItemViewModel MapViewModel(string path, string name, bool isFolder)
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
