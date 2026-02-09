using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides base functionality for file-based services that manage file entities like scripts, stylesheets, and partial views.
/// </summary>
/// <typeparam name="TRepository">The type of repository used for file operations.</typeparam>
/// <typeparam name="TEntity">The type of file entity being managed.</typeparam>
/// <remarks>
///     This abstract class provides common file operations including retrieval, content streaming, and validation
///     for file-based entities stored in the file system.
/// </remarks>
public abstract class FileServiceBase<TRepository, TEntity> : RepositoryService, IBasicFileService<TEntity>
    where TRepository : IFileRepository, IReadRepository<string, TEntity>
    where TEntity : IFile
{
    /// <summary>
    ///     Gets the repository used for file operations.
    /// </summary>
    public TRepository Repository { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileServiceBase{TRepository, TEntity}" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for file data access.</param>
    public FileServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        Repository = repository;
    }

    /// <summary>
    ///     Gets the allowed file extensions for this file type.
    /// </summary>
    protected abstract string[] AllowedFileExtensions { get; }

    /// <summary>
    ///     Determines whether the specified file name has a valid file extension.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><c>true</c> if the file extension is valid; otherwise, <c>false</c>.</returns>
    protected virtual bool HasValidFileExtension(string fileName)
        => AllowedFileExtensions.Contains(Path.GetExtension(fileName));

    /// <summary>
    ///     Determines whether the specified file name is valid.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns><c>true</c> if the file name is valid; otherwise, <c>false</c>.</returns>
    protected virtual bool HasValidFileName(string fileName)
    {
        if (fileName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if a path is considered a root path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path is considered a root path; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     We use "/" here instead of path separator because it's the virtual path used by backoffice, and not an actual file system path.
    /// </remarks>
    protected virtual bool IsRootPath(string? path)
        => string.IsNullOrWhiteSpace(path) || path == "/";

    /// <inheritdoc />
    public Task<TEntity?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(Repository.Get(path));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetAllAsync(params string[] paths)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(Repository.GetMany(paths));
    }

    /// <inheritdoc />
    public Task<Stream> GetContentStreamAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(Repository.GetFileContentStream(path));
    }

    /// <inheritdoc />
    public Task SetContentStreamAsync(string path, Stream content)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        Repository.SetFileContent(path, content);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<long> GetFileSizeAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(Repository.GetFileSize(path));
    }
}
