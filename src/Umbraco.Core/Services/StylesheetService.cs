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
///     Provides services for managing CSS stylesheet files in Umbraco.
/// </summary>
/// <remarks>
///     This service handles CRUD operations for stylesheet files (.css) stored in the file system,
///     including creating, updating, renaming, and deleting stylesheets.
/// </remarks>
public class StylesheetService : FileServiceOperationBase<IStylesheetRepository, IStylesheet, StylesheetOperationStatus>, IStylesheetService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for stylesheet file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditService)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for stylesheet file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository repository,
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
    ///     Initializes a new instance of the <see cref="StylesheetService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for stylesheet file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository repository,
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
    protected override string[] AllowedFileExtensions { get; } = { ".css" };

    /// <inheritdoc />
    protected override StylesheetOperationStatus Success => StylesheetOperationStatus.Success;

    /// <inheritdoc />
    protected override StylesheetOperationStatus NotFound => StylesheetOperationStatus.NotFound;

    /// <inheritdoc />
    protected override StylesheetOperationStatus CancelledByNotification => StylesheetOperationStatus.CancelledByNotification;

    /// <inheritdoc />
    protected override StylesheetOperationStatus PathTooLong => StylesheetOperationStatus.PathTooLong;

    /// <inheritdoc />
    protected override StylesheetOperationStatus AlreadyExists => StylesheetOperationStatus.AlreadyExists;

    /// <inheritdoc />
    protected override StylesheetOperationStatus ParentNotFound => StylesheetOperationStatus.ParentNotFound;

    /// <inheritdoc />
    protected override StylesheetOperationStatus InvalidName => StylesheetOperationStatus.InvalidName;

    /// <inheritdoc />
    protected override StylesheetOperationStatus InvalidFileExtension => StylesheetOperationStatus.InvalidFileExtension;

    /// <inheritdoc />
    protected override string EntityType => "Stylesheet";

    /// <inheritdoc />
    protected override StylesheetSavingNotification SavingNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override StylesheetSavedNotification SavedNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override StylesheetDeletingNotification DeletingNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override StylesheetDeletedNotification DeletedNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override IStylesheet CreateEntity(string path, string? content)
        => new Stylesheet(path) { Content = content };

    /// <inheritdoc />
    public async Task<Attempt<IStylesheet?, StylesheetOperationStatus>> CreateAsync(StylesheetCreateModel createModel, Guid userKey)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath, createModel.Content, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IStylesheet?, StylesheetOperationStatus>> UpdateAsync(string path, StylesheetUpdateModel updateModel, Guid userKey)
        => await HandleUpdateAsync(path, updateModel.Content, userKey);

    /// <inheritdoc />
    public async Task<StylesheetOperationStatus> DeleteAsync(string path, Guid userKey)
        => await HandleDeleteAsync(path, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IStylesheet?, StylesheetOperationStatus>> RenameAsync(string path, StylesheetRenameModel renameModel, Guid userKey)
        => await HandleRenameAsync(path, renameModel.Name, userKey);
}
