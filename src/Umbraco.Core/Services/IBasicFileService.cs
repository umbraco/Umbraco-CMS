using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides basic file operations for entities that implement <see cref="IFile"/>.
/// </summary>
/// <typeparam name="TEntity">The type of file entity.</typeparam>
public interface IBasicFileService<TEntity> where TEntity : IFile
{
    /// <summary>
    /// Gets <typeparamref name="TEntity"/> by path.
    /// </summary>
    /// <param name="path">The path to get <typeparamref name="TEntity"/> from.</param>
    /// <returns><typeparamref name="TEntity"/>, or null if not found.</returns>
    Task<TEntity?> GetAsync(string path);

    /// <summary>
    /// Gets all <typeparamref name="TEntity"/> by path, or all if no paths are specified.
    /// </summary>
    /// <param name="paths">Optional paths of <typeparamref name="TEntity"/> to get.</param>
    /// <returns>IEnumerable of <typeparamref name="TEntity"/>.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(params string[] paths);

    /// <summary>
    /// Get the content of a file as a stream.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A stream containing the contents of the file.</returns>
    Task<Stream> GetContentStreamAsync(string path);

    /// <summary>
    /// Set the content of a file from a stream.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="content">The desired content of the file as a stream.</param>
    Task SetContentStreamAsync(string path, Stream content);

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The file size in bytes.</returns>
    Task<long> GetFileSizeAsync(string path);
}
