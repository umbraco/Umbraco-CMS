using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Snippets;
using PartialViewSnippet = Umbraco.Cms.Core.Snippets.PartialViewSnippet;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing partial view files in Umbraco.
/// </summary>
/// <remarks>
///     This service handles CRUD operations for partial view files (.cshtml) stored in the file system,
///     including creating, updating, renaming, and deleting partial views. It also provides access to
///     partial view snippets that can be used as templates when creating new partial views.
/// </remarks>
public class PartialViewService : FileServiceOperationBase<IPartialViewRepository, IPartialView, PartialViewOperationStatus>, IPartialViewService
{
    private readonly PartialViewSnippetCollection _snippetCollection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for partial view file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="snippetCollection">The collection of available partial view snippets.</param>
    public PartialViewService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPartialViewRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService,
        PartialViewSnippetCollection snippetCollection)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditService)
        => _snippetCollection = snippetCollection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for partial view file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    /// <param name="snippetCollection">The collection of available partial view snippets.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public PartialViewService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPartialViewRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditRepository auditRepository,
        PartialViewSnippetCollection snippetCollection)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            logger,
            userIdKeyResolver,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            snippetCollection)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider for managing database transactions.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="repository">The repository for partial view file operations.</param>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user keys to IDs.</param>
    /// <param name="auditService">The service for audit logging.</param>
    /// <param name="auditRepository">The repository for audit logging (obsolete).</param>
    /// <param name="snippetCollection">The collection of available partial view snippets.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public PartialViewService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPartialViewRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService,
        IAuditRepository auditRepository,
        PartialViewSnippetCollection snippetCollection)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            logger,
            userIdKeyResolver,
            auditService,
            snippetCollection)
    {
    }

    /// <inheritdoc />
    protected override string[] AllowedFileExtensions { get; } = { ".cshtml" };

    /// <inheritdoc />
    protected override PartialViewOperationStatus Success => PartialViewOperationStatus.Success;

    /// <inheritdoc />
    protected override PartialViewOperationStatus NotFound => PartialViewOperationStatus.NotFound;

    /// <inheritdoc />
    protected override PartialViewOperationStatus CancelledByNotification => PartialViewOperationStatus.CancelledByNotification;

    /// <inheritdoc />
    protected override PartialViewOperationStatus PathTooLong => PartialViewOperationStatus.PathTooLong;

    /// <inheritdoc />
    protected override PartialViewOperationStatus AlreadyExists => PartialViewOperationStatus.AlreadyExists;

    /// <inheritdoc />
    protected override PartialViewOperationStatus ParentNotFound => PartialViewOperationStatus.ParentNotFound;

    /// <inheritdoc />
    protected override PartialViewOperationStatus InvalidName => PartialViewOperationStatus.InvalidName;

    /// <inheritdoc />
    protected override PartialViewOperationStatus InvalidFileExtension => PartialViewOperationStatus.InvalidFileExtension;

    /// <inheritdoc />
    protected override string EntityType => "PartialView";

    /// <inheritdoc />
    protected override PartialViewSavingNotification SavingNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override PartialViewSavedNotification SavedNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override PartialViewDeletingNotification DeletingNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override PartialViewDeletedNotification DeletedNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    /// <inheritdoc />
    protected override IPartialView CreateEntity(string path, string? content)
        => new PartialView(path) { Content = content };

    /// <inheritdoc />
    public Task<PagedModel<PartialViewSnippetSlim>> GetSnippetsAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        var result = new PagedModel<PartialViewSnippetSlim>(
            _snippetCollection.Count,
            _snippetCollection
                .Skip(skip)
                .Take(take)
                .Select(snippet => new PartialViewSnippetSlim(snippet.Id, snippet.Name))
                .ToArray());
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<PartialViewSnippet?> GetSnippetAsync(string id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        PartialViewSnippet? snippet = _snippetCollection.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(snippet);
    }

    /// <inheritdoc />
    public async Task<PartialViewOperationStatus> DeleteAsync(string path, Guid userKey)
        => await HandleDeleteAsync(path, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> CreateAsync(PartialViewCreateModel createModel, Guid userKey)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath, createModel.Content, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> UpdateAsync(string path, PartialViewUpdateModel updateModel, Guid userKey)
        => await HandleUpdateAsync(path, updateModel.Content, userKey);

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> RenameAsync(string path, PartialViewRenameModel renameModel, Guid userKey)
        => await HandleRenameAsync(path, renameModel.Name, userKey);
}
