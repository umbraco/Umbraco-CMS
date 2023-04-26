using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public abstract class PathFolderServiceBase<TRepo, TStatus> :
    RepositoryService,
    IPathFolderService<TStatus>
    where TRepo: IFileWithFoldersRepository
    where TStatus : Enum
{
    protected PathFolderServiceBase(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory) : base(provider, loggerFactory, eventMessagesFactory)
    {
    }

    protected abstract TRepo Repository { get; }

    protected abstract TStatus SuccessStatus { get; }

    public virtual Task<PathContainer?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // There's not much we can actually get when it's a folder, so it more a matter of ensuring the folder exists and returning a model.
        if (Repository.FolderExists(path) is false)
        {
            return Task.FromResult<PathContainer?>(null);
        }

        PathContainer model = CreateFromPath(path);

        scope.Complete();
        return Task.FromResult<PathContainer?>(model);
    }

    public async Task<Attempt<PathContainer?, TStatus>> CreateAsync(PathContainer container)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<TStatus> validationResult = await ValidateCreateAsync(container);
        if (validationResult.Success is false)
        {
            return Attempt.FailWithStatus<PathContainer?, TStatus>(validationResult.Result!, null);
        }

        Repository.AddFolder(container.Path);

        scope.Complete();

        return Attempt.SucceedWithStatus<PathContainer?, TStatus>(SuccessStatus, CreateFromPath(container.Path));
    }

    protected abstract Task<Attempt<TStatus>> ValidateCreateAsync(PathContainer container);

    public async Task<Attempt<TStatus?>> DeleteAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<TStatus> validationResult = await ValidateDeleteAsync(path);
        if (validationResult.Success is false)
        {
            return Attempt.Fail(validationResult.Result);
        }

        Repository.DeleteFolder(path);

        scope.Complete();

        return Attempt.Succeed(SuccessStatus);
    }

    protected abstract Task<Attempt<TStatus>> ValidateDeleteAsync(string path);

    private PathContainer CreateFromPath(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        var parentPathLength = string.IsNullOrEmpty(parentPath) ? 0 : parentPath.Length + 1;

        var model = new PathContainer
        {
            Name = path.Remove(0, parentPathLength),
            ParentPath = parentPath,
        };

        return model;
    }
}
