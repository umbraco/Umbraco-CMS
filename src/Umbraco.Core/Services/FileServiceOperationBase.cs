using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public abstract class FileServiceOperationBase<TRepository, TEntity, TOperationStatus> : FileServiceBase<TRepository, TEntity>
    where TRepository : IFileRepository, IReadRepository<string, TEntity>, IWriteRepository<TEntity>, IFileWithFoldersRepository
    where TEntity : IFile
    where TOperationStatus : Enum
{
    private readonly ILogger<StylesheetService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IAuditRepository _auditRepository;

    protected FileServiceOperationBase(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, TRepository repository, ILogger<StylesheetService> logger, IUserIdKeyResolver userIdKeyResolver, IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory, repository)
    {
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _auditRepository = auditRepository;
    }

    protected abstract TOperationStatus Success { get; }

    protected abstract TOperationStatus NotFound { get; }

    protected abstract TOperationStatus CancelledByNotification { get; }

    protected abstract TOperationStatus PathTooLong { get; }

    protected abstract TOperationStatus AlreadyExists { get; }

    protected abstract TOperationStatus ParentNotFound { get; }

    protected abstract TOperationStatus InvalidName { get; }

    protected abstract TOperationStatus InvalidFileExtension { get; }

    protected abstract string EntityType { get; }

    protected abstract SavingNotification<TEntity> SavingNotification(TEntity target, EventMessages messages);

    protected abstract SavedNotification<TEntity> SavedNotification(TEntity target, EventMessages messages);

    protected abstract DeletingNotification<TEntity> DeletingNotification(TEntity target, EventMessages messages);

    protected abstract DeletedNotification<TEntity> DeletedNotification(TEntity target, EventMessages messages);

    protected abstract TEntity CreateEntity(string path, string? content);

    protected async Task<TOperationStatus> HandleDeleteAsync(string path, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        TEntity? entity = Repository.Get(path);
        if (entity is null)
        {
            return NotFound;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        DeletingNotification<TEntity> deletingNotification = DeletingNotification(entity, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            return CancelledByNotification;
        }

        Repository.Delete(entity);

        scope.Notifications.Publish(DeletedNotification(entity, eventMessages).WithStateFrom(deletingNotification));
        await AuditAsync(AuditType.Delete, userKey);

        scope.Complete();
        return Success;
    }

    protected async Task<Attempt<TEntity?, TOperationStatus>> HandleCreateAsync(string name, string? parentPath, string? content, Guid userKey)
    {
        if (name.Contains('/'))
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(InvalidName, default);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var path = GetFilePath(parentPath, name);
        try
        {
            TOperationStatus validationResult = ValidateCreate(path);
            if (Success.Equals(validationResult) is false)
            {
                return Attempt.FailWithStatus<TEntity?, TOperationStatus>(validationResult, default);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The {EntityType} path was too long", EntityType);
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(PathTooLong, default);
        }

        TEntity entity = CreateEntity(path, content);

        EventMessages eventMessages = EventMessagesFactory.Get();
        SavingNotification<TEntity> savingNotification = SavingNotification(entity, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(CancelledByNotification, default);
        }

        Repository.Save(entity);

        scope.Notifications.Publish(SavedNotification(entity, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, userKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<TEntity?, TOperationStatus>(Success, entity);
    }

    protected async Task<Attempt<TEntity?, TOperationStatus>> HandleUpdateAsync(string path, string content, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        TEntity? entity = Repository.Get(path);

        if (entity is null)
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(NotFound, default);
        }

        entity.Content = content;

        EventMessages eventMessages = EventMessagesFactory.Get();
        SavingNotification<TEntity> savingNotification = SavingNotification(entity, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(CancelledByNotification, default);
        }

        Repository.Save(entity);

        scope.Notifications.Publish(SavedNotification(entity, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, userKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<TEntity?, TOperationStatus>(Success, entity);
    }

    protected async Task<Attempt<TEntity?, TOperationStatus>> HandleRenameAsync(string path, string newName, Guid userKey)
    {
        if (newName.Contains('/'))
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(InvalidName, default);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        TEntity? entity = Repository.Get(path);

        if (entity is null)
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(NotFound, default);
        }

        var newPath = ReplaceFileName(path, newName);

        try
        {
            TOperationStatus validationResult = ValidateRename(newName, newPath);
            if (Success.Equals(validationResult) is false)
            {
                return Attempt.FailWithStatus<TEntity?, TOperationStatus>(validationResult, default);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The {EntityType} path was too long", EntityType);
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(PathTooLong, default);
        }

        entity.Path = newPath;

        EventMessages eventMessages = EventMessagesFactory.Get();
        SavingNotification<TEntity> savingNotification = SavingNotification(entity, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<TEntity?, TOperationStatus>(CancelledByNotification, default);
        }

        Repository.Save(entity);

        scope.Notifications.Publish(SavedNotification(entity, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, userKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<TEntity?, TOperationStatus>(Success, entity);
    }

    private TOperationStatus ValidateCreate(string path)
    {
        if (Repository.Exists(path))
        {
            return AlreadyExists;
        }

        var directoryPath = GetDirectoryPath(path);
        if (directoryPath.IsNullOrWhiteSpace() is false && Repository.FolderExists(directoryPath) is false)
        {
            return ParentNotFound;
        }

        var fileName = GetFileName(path);
        if (HasValidFileName(fileName) is false)
        {
            return InvalidName;
        }

        if (HasValidFileExtension(fileName) is false)
        {
            return InvalidFileExtension;
        }

        return Success;
    }

    private TOperationStatus ValidateRename(string newName, string newPath)
    {
        if (Repository.Exists(newPath))
        {
            return AlreadyExists;
        }

        if (HasValidFileName(newName) is false)
        {
            return InvalidName;
        }

        if (HasValidFileExtension(newName) is false)
        {
            return InvalidFileExtension;
        }

        return Success;
    }

    private async Task AuditAsync(AuditType type, Guid userKey)
    {
        var userId = await _userIdKeyResolver.GetAsync(userKey);
        _auditRepository.Save(new AuditItem(-1, type, userId, EntityType));
    }

    private string GetFilePath(string? parentPath, string fileName)
        => Path.Join(parentPath, fileName);

    private string ReplaceFileName(string path, string newName)
        => Path.Join(GetDirectoryPath(path), newName);

    private string GetDirectoryPath(string path)
        => Path.GetDirectoryName(path) ?? string.Empty;

    private string GetFileName(string path)
        => Path.GetFileName(path);
}
