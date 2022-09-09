using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers;

public abstract class FileSystemTreeControllerBase : Controller
{
    protected abstract IFileSystem FileSystem { get; }

    protected abstract string FileIcon(string path);

    protected abstract string ItemType(string path);

    protected async Task<ActionResult<PagedResult<FileSystemTreeItemViewModel>>> GetRoot(long pageNumber = 0, int pageSize = 100)
    {
        FileSystemTreeItemViewModel[] viewModels = GetPathViewModels(string.Empty);

        PagedResult<FileSystemTreeItemViewModel> result = PagedResult(viewModels, 0, viewModels.Length, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedResult<FileSystemTreeItemViewModel>>> GetChildren(string path, long pageNumber = 0, int pageSize = 100)
    {
        FileSystemTreeItemViewModel[] viewModels = GetPathViewModels(path);

        PagedResult<FileSystemTreeItemViewModel> result = PagedResult(viewModels, 0, viewModels.Length, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedResult<FileSystemTreeItemViewModel>>> GetItems(string[] paths)
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

        PagedResult<FileSystemTreeItemViewModel> result = PagedResult(viewModels, 0, viewModels.Length, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }

    protected virtual string[] GetDirectories(string path) => FileSystem.GetDirectories(path).ToArray();

    protected virtual string[] GetFiles(string path) => FileSystem.GetFiles(path).ToArray();

    protected virtual string GetFileName(string path) => FileSystem.GetFileName(path);

    protected virtual bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    private FileSystemTreeItemViewModel[] GetPathViewModels(string path)
    {
        var directories = GetDirectories(path).ToArray();
        var files = GetFiles(path).ToArray();

        FileSystemTreeItemViewModel ViewModel(string itemPath, bool isFolder)
            => MapViewModel(
                itemPath,
                isFolder ? Path.GetFileName(itemPath) : FileSystem.GetFileName(itemPath),
                isFolder);

        return directories
            .Select(directory => ViewModel(directory, true))
            .Union(files.Select(file => ViewModel(file, false)))
            .ToArray();
    }

    private PagedResult<FileSystemTreeItemViewModel> PagedResult(IEnumerable<FileSystemTreeItemViewModel> viewModels, long pageNumber, int pageSize, long totalItems)
        => new(totalItems, pageNumber, pageSize) { Items = viewModels };

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
