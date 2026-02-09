using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides base functionality for file services that support CRUD operations with notifications and auditing.
/// </summary>
/// <typeparam name="TRepository">The type of repository used for file operations, supporting read, write, and folder operations.</typeparam>
/// <typeparam name="TEntity">The type of file entity being managed.</typeparam>
/// <typeparam name="TOperationStatus">The enum type representing operation status codes.</typeparam>
/// <remarks>
///     This abstract class extends <see cref="FileServiceBase{TRepository, TEntity}" /> to add support for
///     create, update, rename, and delete operations with proper notification publishing and audit logging.
/// </remarks>
public abstract class FileServiceOperationBase<TRepository, TEntity, TOperationStatus> : FileServiceBase<TRepository, TEntity>
    where TRepository : IFileRepository, IReadRepository<string, TEntity>, IWriteRepository<TEntity>, IFileWithFoldersRepository
    where TEntity : IFile
    where TOperationStatus : Enum
{
    private readonly ILogger<StylesheetService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IAuditService _auditService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileServiceOperationBase{TRepository, TEntity, TOperationStatus}" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for file data access.</param>
    /// <param name="logger">The logger for operation logging.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    /// <param name="auditService">The audit service for logging operations.</param>
    protected FileServiceOperationBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService)
        : base(provider, loggerFactory, eventMessagesFactory, repository)
    {
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _auditService = auditService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileServiceOperationBase{TRepository, TEntity, TOperationStatus}"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="repository">The file repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="auditRepository">The audit repository.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    protected FileServiceOperationBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditRepository auditRepository)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            logger,
            userIdKeyResolver,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>())
    {
    }

    /// <summary>
    ///     Gets the operation status value representing a successful operation.
    /// </summary>
    protected abstract TOperationStatus Success { get; }

    /// <summary>
    ///     Gets the operation status value representing an entity not found condition.
    /// </summary>
    protected abstract TOperationStatus NotFound { get; }

    /// <summary>
    ///     Gets the operation status value representing a cancellation by notification handler.
    /// </summary>
    protected abstract TOperationStatus CancelledByNotification { get; }

    /// <summary>
    ///     Gets the operation status value representing a path that exceeds maximum length.
    /// </summary>
    protected abstract TOperationStatus PathTooLong { get; }

    /// <summary>
    ///     Gets the operation status value representing an entity that already exists.
    /// </summary>
    protected abstract TOperationStatus AlreadyExists { get; }

    /// <summary>
    ///     Gets the operation status value representing a parent folder not found condition.
    /// </summary>
    protected abstract TOperationStatus ParentNotFound { get; }

    /// <summary>
    ///     Gets the operation status value representing an invalid file name.
    /// </summary>
    protected abstract TOperationStatus InvalidName { get; }

    /// <summary>
    ///     Gets the operation status value representing an invalid file extension.
    /// </summary>
    protected abstract TOperationStatus InvalidFileExtension { get; }

    /// <summary>
    ///     Gets the entity type name used for audit logging.
    /// </summary>
    protected abstract string EntityType { get; }

    /// <summary>
    ///     Creates a saving notification for the specified entity.
    /// </summary>
    /// <param name="target">The entity being saved.</param>
    /// <param name="messages">The event messages to include.</param>
    /// <returns>A saving notification instance.</returns>
    protected abstract SavingNotification<TEntity> SavingNotification(TEntity target, EventMessages messages);

    /// <summary>
    ///     Creates a saved notification for the specified entity.
    /// </summary>
    /// <param name="target">The entity that was saved.</param>
    /// <param name="messages">The event messages to include.</param>
    /// <returns>A saved notification instance.</returns>
    protected abstract SavedNotification<TEntity> SavedNotification(TEntity target, EventMessages messages);

    /// <summary>
    ///     Creates a deleting notification for the specified entity.
    /// </summary>
    /// <param name="target">The entity being deleted.</param>
    /// <param name="messages">The event messages to include.</param>
    /// <returns>A deleting notification instance.</returns>
    protected abstract DeletingNotification<TEntity> DeletingNotification(TEntity target, EventMessages messages);

    /// <summary>
    ///     Creates a deleted notification for the specified entity.
    /// </summary>
    /// <param name="target">The entity that was deleted.</param>
    /// <param name="messages">The event messages to include.</param>
    /// <returns>A deleted notification instance.</returns>
    protected abstract DeletedNotification<TEntity> DeletedNotification(TEntity target, EventMessages messages);

    /// <summary>
    ///     Creates a new entity instance with the specified path and content.
    /// </summary>
    /// <param name="path">The file path for the entity.</param>
    /// <param name="content">The optional content of the file.</param>
    /// <returns>A new entity instance.</returns>
    protected abstract TEntity CreateEntity(string path, string? content);

    /// <summary>
    ///     Handles the deletion of a file entity.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>The operation status indicating success or failure reason.</returns>
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

    /// <summary>
    ///     Handles the creation of a new file entity.
    /// </summary>
    /// <param name="name">The name of the file to create.</param>
    /// <param name="parentPath">The optional parent folder path.</param>
    /// <param name="content">The optional initial content of the file.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>An attempt containing the created entity and operation status.</returns>
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

    /// <summary>
    ///     Handles the update of an existing file entity's content.
    /// </summary>
    /// <param name="path">The path of the file to update.</param>
    /// <param name="content">The new content for the file.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>An attempt containing the updated entity and operation status.</returns>
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

    /// <summary>
    ///     Handles the renaming of an existing file entity.
    /// </summary>
    /// <param name="path">The current path of the file.</param>
    /// <param name="newName">The new name for the file.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>An attempt containing the renamed entity and operation status.</returns>
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

    /// <summary>
    /// Validates that a file can be created at the specified path.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>The operation status indicating success or the validation failure reason.</returns>
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

    /// <summary>
    /// Validates that a file can be renamed with the specified new name.
    /// </summary>
    /// <param name="newName">The new name for the file.</param>
    /// <param name="newPath">The new path after renaming.</param>
    /// <returns>The operation status indicating success or the validation failure reason.</returns>
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

    /// <summary>
    /// Creates an audit entry for the specified operation.
    /// </summary>
    /// <param name="type">The type of audit entry.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    private async Task AuditAsync(AuditType type, Guid userKey)
        => await _auditService.AddAsync(type, userKey, -1, EntityType);

    /// <summary>
    /// Combines a parent path and file name into a full file path.
    /// </summary>
    /// <param name="parentPath">The parent directory path, or <c>null</c> for root.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The combined file path.</returns>
    private string GetFilePath(string? parentPath, string fileName)
        => Path.Join(parentPath, fileName);

    /// <summary>
    /// Replaces the file name in a path with a new name.
    /// </summary>
    /// <param name="path">The current file path.</param>
    /// <param name="newName">The new file name.</param>
    /// <returns>The path with the replaced file name.</returns>
    private string ReplaceFileName(string path, string newName)
        => Path.Join(GetDirectoryPath(path), newName);

    /// <summary>
    /// Gets the directory path from a full file path.
    /// </summary>
    /// <param name="path">The full file path.</param>
    /// <returns>The directory path, or an empty string if not found.</returns>
    private string GetDirectoryPath(string path)
        => Path.GetDirectoryName(path) ?? string.Empty;

    /// <summary>
    /// Gets the file name from a full file path.
    /// </summary>
    /// <param name="path">The full file path.</param>
    /// <returns>The file name.</returns>
    private string GetFileName(string path)
        => Path.GetFileName(path);
}
