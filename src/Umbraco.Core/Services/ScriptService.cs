using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing JavaScript script files in Umbraco.
/// </summary>
/// <remarks>
///     This service handles CRUD operations for script files (.js) stored in the file system,
///     including creating, updating, renaming, and deleting scripts.
/// </remarks>
public class ScriptService : FileServiceOperationBase<IScriptRepository, IScript, ScriptOperationStatus>, IScriptService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for script file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditService)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for script file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository repository,
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
    ///     Initializes a new instance of the <see cref="ScriptService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for script file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService,
        IAuditRepository auditRepository)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            logger,
            userIdKeyResolver,
            auditService)
    {
    }

    /// <inheritdoc />
    protected override string[] AllowedFileExtensions { get; } = { ".js" };

    /// <inheritdoc />
    protected override ScriptOperationStatus Success => ScriptOperationStatus.Success;

    /// <inheritdoc />
    protected override ScriptOperationStatus NotFound => ScriptOperationStatus.NotFound;

    /// <inheritdoc />
    protected override ScriptOperationStatus CancelledByNotification => ScriptOperationStatus.CancelledByNotification;

    /// <inheritdoc />
    protected override ScriptOperationStatus PathTooLong => ScriptOperationStatus.PathTooLong;

    /// <inheritdoc />
    protected override ScriptOperationStatus AlreadyExists => ScriptOperationStatus.AlreadyExists;

    /// <inheritdoc />
    protected override ScriptOperationStatus ParentNotFound => ScriptOperationStatus.ParentNotFound;

    /// <inheritdoc />
    protected override ScriptOperationStatus InvalidName => ScriptOperationStatus.InvalidName;

    /// <inheritdoc />
    protected override ScriptOperationStatus InvalidFileExtension => ScriptOperationStatus.InvalidFileExtension;

    /// <inheritdoc />
    protected override string EntityType => "Script";

    /// <inheritdoc />
    protected override ScriptSavingNotification SavingNotification(IScript target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override ScriptSavedNotification SavedNotification(IScript target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override ScriptDeletingNotification DeletingNotification(IScript target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override ScriptDeletedNotification DeletedNotification(IScript target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override IScript CreateEntity(string path, string? content)
        => new Script(path) { Content = content };

    /// <inheritdoc />
    public async Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid userKey)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath, createModel.Content, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IScript?, ScriptOperationStatus>> UpdateAsync(string path, ScriptUpdateModel updateModel, Guid userKey)
        => await HandleUpdateAsync(path, updateModel.Content, userKey);

    /// <inheritdoc />
    public async Task<ScriptOperationStatus> DeleteAsync(string path, Guid userKey)
        => await HandleDeleteAsync(path, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IScript?, ScriptOperationStatus>> RenameAsync(string path, ScriptRenameModel renameModel, Guid userKey)
        => await HandleRenameAsync(path, renameModel.Name, userKey);
}
