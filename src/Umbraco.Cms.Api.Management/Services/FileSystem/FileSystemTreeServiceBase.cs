using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public abstract class FileSystemTreeServiceBase : IFileSystemTreeService
{
    protected abstract IFileSystem FileSystem { get; }

    public FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf)
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

    public FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems)
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

    public FileSystemTreeItemPresentationModel[] GetSiblingsViewModels(string path, int before, int after, out long totalBefore, out long totalAfter)
    {
        var filePath = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);

        FileSystemTreeItemPresentationModel[] viewModels = GetPathViewModels(filePath!, 0, int.MaxValue, out totalBefore);
        FileSystemTreeItemPresentationModel? target = viewModels.FirstOrDefault(item => item.Name == fileName);
        var position = Array.IndexOf(viewModels, target);

        totalBefore = position - before < 0 ? 0 : position - before;
        totalAfter = (viewModels.Length - 1) - (position + after) < 0 ? 0 : (viewModels.Length - 1) - (position + after);

        return viewModels
            .Select((item, index) => new { item, index })
            .Where(item => item.index >= position - before && item.index <= position + after)
            .Select(item => item.item)
            .ToArray();
    }

    public virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    public virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .Where(FilterFile)
        .OrderBy(file => file)
        .ToArray();

    protected virtual bool FilterFile(string file) => true;

    public bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    public string GetFileSystemItemName(bool isFolder, string itemPath) => isFolder
        ? Path.GetFileName(itemPath)
        : FileSystem.GetFileName(itemPath);

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
