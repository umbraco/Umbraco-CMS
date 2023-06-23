using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IBasicFileService<TEntity> where TEntity : IFile
{
    /// <summary>
    /// Gets <see cref="TEntity"/> by path.
    /// </summary>
    /// <param name="path">The path to get <see cref="TEntity"/> from.</param>
    /// <returns><see cref="TEntity"/>, or null if not found</returns>
    Task<TEntity?> GetAsync(string path);

    /// <summary>
    /// Gets all <see cref="TEntity"/> by path, or all if no paths are specified.
    /// </summary>
    /// <param name="paths">Optional paths of <see cref="TEntity"/> to get.</param>
    /// <returns>IEnumerable of <see cref="TEntity"/></returns>
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
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<long> GetFileSizeAsync(string path);
}
