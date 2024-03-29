using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public abstract class FileServiceBase<TRepository, TEntity> : RepositoryService, IBasicFileService<TEntity>
    where TRepository : IFileRepository, IReadRepository<string, TEntity>
    where TEntity : IFile
{
    public TRepository Repository { get; }

    public FileServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        Repository = repository;
    }

    protected abstract string[] AllowedFileExtensions { get; }

    protected virtual bool HasValidFileExtension(string fileName)
        => AllowedFileExtensions.Contains(Path.GetExtension(fileName));

    protected virtual bool HasValidFileName(string fileName)
    {
        if (fileName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a path is considered a root path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is considered a root path.</returns>
    protected virtual bool IsRootPath(string? path)
    // We use "/" here instead of path separator because it's the virtual path used by backoffice, and not an actual FS path.
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
