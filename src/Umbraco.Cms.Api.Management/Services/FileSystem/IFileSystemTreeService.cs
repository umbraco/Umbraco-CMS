using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Defines the contract for a service responsible for managing and interacting with file system trees.
/// </summary>
public interface IFileSystemTreeService
{
    /// <summary>
    /// Gets the ancestor models for the specified path in the file system tree.
    /// </summary>
    /// <param name="path">The path to get ancestors for.</param>
    /// <param name="includeSelf">Whether to include the item at the specified path itself in the results.</param>
    /// <returns>An array of <see cref="Umbraco.Cms.Api.Management.Services.FileSystem.FileSystemTreeItemPresentationModel"/> representing the ancestor items.</returns>
    FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf);

    /// <summary>
    /// Retrieves a paginated collection of <see cref="FileSystemTreeItemPresentationModel"/> items located at the specified path in the file system.
    /// </summary>
    /// <param name="path">The file system path from which to retrieve items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (for pagination).</param>
    /// <param name="take">The maximum number of items to return (for pagination).</param>
    /// <param name="totalItems">When this method returns, contains the total number of items available at the specified path, regardless of pagination.</param>
    /// <returns>An array of <see cref="FileSystemTreeItemPresentationModel"/> representing the items at the specified path.</returns>
    FileSystemTreeItemPresentationModel[] GetPathViewModels(string path, int skip, int take, out long totalItems);

    /// <summary>
    /// Retrieves sibling items in the file system tree for the specified path.
    /// </summary>
    /// <param name="path">The path whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to retrieve before the specified path.</param>
    /// <param name="after">The number of sibling items to retrieve after the specified path.</param>
    /// <param name="totalBefore">When this method returns, contains the total number of siblings before the specified path.</param>
    /// <param name="totalAfter">When this method returns, contains the total number of siblings after the specified path.</param>
    /// <returns>An array of <see cref="FileSystemTreeItemPresentationModel"/> representing the sibling items.</returns>
    FileSystemTreeItemPresentationModel[] GetSiblingsViewModels(string path, int before, int after, out long totalBefore, out long totalAfter);

    /// <summary>
    /// Retrieves the names of all directories located at the specified path.
    /// </summary>
    /// <param name="path">The path from which to retrieve directory names.</param>
    /// <returns>An array containing the names of the directories at the specified path.</returns>
    string[] GetDirectories(string path);

    /// <summary>
    /// Retrieves the file names located at the specified path in the file system.
    /// </summary>
    /// <param name="path">The path from which to retrieve file names.</param>
    /// <returns>An array of file names found at the specified path.</returns>
    string[] GetFiles(string path);
}
