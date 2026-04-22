using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Serves as a base class for services that manage file system trees in the Umbraco CMS Management API.
/// </summary>
public abstract class FileSystemTreeServiceBase : IFileSystemTreeService
{
    protected abstract IFileSystem FileSystem { get; }

    /// <summary>
    /// Retrieves the ancestor models for a given file system path, optionally including the model for the path itself.
    /// </summary>
    /// <param name="path">The file system path for which to retrieve ancestor models.</param>
    /// <param name="includeSelf">If <c>true</c>, includes the model representing the specified path itself as the last element in the result; otherwise, only ancestors are included.</param>
    /// <returns>
    /// An array of <see cref="Umbraco.Cms.Api.Management.Services.FileSystem.FileSystemTreeItemPresentationModel"/> objects, each representing a directory in the ancestor chain of the specified path, ordered from the root to the specified path (if <paramref name="includeSelf"/> is <c>true</c>).
    /// </returns>
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

    /// <summary>
    /// Retrieves a paged array of file system tree item presentation models for the specified virtual path.
    /// </summary>
    /// <param name="path">The virtual path from which to retrieve directory and file items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <param name="totalItems">When this method returns, contains the total number of items available at the specified path, before paging is applied.</param>
    /// <returns>
    /// An array of <see cref="FileSystemTreeItemPresentationModel"/> objects representing the directories and files at the specified path, limited by the paging parameters.
    /// </returns>
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

    /// <summary>
    /// Retrieves a range of sibling view models surrounding the item at the specified path.
    /// </summary>
    /// <param name="path">The path of the item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target item (preceding siblings).</param>
    /// <param name="after">The number of sibling items to include after the target item (following siblings).</param>
    /// <param name="totalBefore">When this method returns, contains the total number of siblings before the returned range.</param>
    /// <param name="totalAfter">When this method returns, contains the total number of siblings after the returned range.</param>
    /// <returns>An array of <see cref="FileSystemTreeItemPresentationModel"/> representing the siblings before and after the specified item, including the item itself.</returns>
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

    /// <summary>
    /// Retrieves the names of all directories located at the specified path, ordered alphabetically.
    /// </summary>
    /// <param name="path">The file system path from which to retrieve directory names.</param>
    /// <returns>An array of directory names found at the specified path, sorted alphabetically.</returns>
    public virtual string[] GetDirectories(string path) => FileSystem
        .GetDirectories(path)
        .OrderBy(directory => directory)
        .ToArray();

    /// <summary>
    /// Gets the files at the specified path, filtered using <see cref="FilterFile"/> and ordered alphabetically.
    /// </summary>
    /// <param name="path">The path from which to retrieve files.</param>
    /// <returns>An array of file names that match the filter, ordered alphabetically.</returns>
    public virtual string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .Where(FilterFile)
        .OrderBy(file => file)
        .ToArray();

    protected virtual bool FilterFile(string file) => true;

    /// <summary>
    /// Determines whether the specified directory contains any files or subdirectories.
    /// </summary>
    /// <param name="path">The path of the directory to check.</param>
    /// <returns><c>true</c> if the directory has any children; otherwise, <c>false</c>.</returns>
    public bool DirectoryHasChildren(string path)
        => FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

    /// <summary>
    /// Gets the name of the file system item from the specified path.
    /// </summary>
    /// <param name="isFolder">Indicates whether the item is a folder.</param>
    /// <param name="itemPath">The path of the file system item.</param>
    /// <returns>The name of the file system item.</returns>
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
