using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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

public class PartialViewService : FileServiceOperationBase<IPartialViewRepository, IPartialView, PartialViewOperationStatus>, IPartialViewService
{
    private readonly PartialViewSnippetCollection _snippetCollection;
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;

    // TODO (V18): Remove obsolete constructors and the ActivatorUtilitiesConstructor attribute.
    // Also update UmbracoBuilder where this service is registered using:
    //   Services.AddUnique<IPartialViewService>(sp => ActivatorUtilities.CreateInstance<PartialViewService>(sp));
    // We do this to allow the ActivatorUtilitiesConstructor to be used (it's otherwise ignored by AddUnique).
    // Revert it to:
    //   Services.AddUnique<IPartialViewService, PartialViewService>();

    [ActivatorUtilitiesConstructor]
    public PartialViewService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPartialViewRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditService auditService,
        PartialViewSnippetCollection snippetCollection,
        IOptionsMonitor<RuntimeSettings> runtimeSettings)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditService)
    {
        _snippetCollection = snippetCollection;
        _runtimeSettings = runtimeSettings;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in v19.")]
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
            snippetCollection,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<RuntimeSettings>>())
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in v19.")]
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
            snippetCollection,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<RuntimeSettings>>())
    {
    }

    protected override string[] AllowedFileExtensions { get; } = { ".cshtml" };

    protected override PartialViewOperationStatus Success => PartialViewOperationStatus.Success;

    protected override PartialViewOperationStatus NotFound => PartialViewOperationStatus.NotFound;

    protected override PartialViewOperationStatus CancelledByNotification => PartialViewOperationStatus.CancelledByNotification;

    protected override PartialViewOperationStatus PathTooLong => PartialViewOperationStatus.PathTooLong;

    protected override PartialViewOperationStatus AlreadyExists => PartialViewOperationStatus.AlreadyExists;

    protected override PartialViewOperationStatus ParentNotFound => PartialViewOperationStatus.ParentNotFound;

    protected override PartialViewOperationStatus InvalidName => PartialViewOperationStatus.InvalidName;

    protected override PartialViewOperationStatus InvalidFileExtension => PartialViewOperationStatus.InvalidFileExtension;

    protected override string EntityType => "PartialView";

    protected override PartialViewSavingNotification SavingNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    protected override PartialViewSavedNotification SavedNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    protected override PartialViewDeletingNotification DeletingNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

    protected override PartialViewDeletedNotification DeletedNotification(IPartialView target, EventMessages messages)
        => new(target, messages);

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
    {
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return PartialViewOperationStatus.NotAllowedInProductionMode;
        }

        return await HandleDeleteAsync(path, userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> CreateAsync(PartialViewCreateModel createModel, Guid userKey)
    {
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.NotAllowedInProductionMode, null);
        }

        return await HandleCreateAsync(createModel.Name, createModel.ParentPath, createModel.Content, userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> UpdateAsync(string path, PartialViewUpdateModel updateModel, Guid userKey)
    {
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.NotAllowedInProductionMode, null);
        }

        return await HandleUpdateAsync(path, updateModel.Content, userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> RenameAsync(string path, PartialViewRenameModel renameModel, Guid userKey)
    {
        if (_runtimeSettings.CurrentValue.Mode == RuntimeMode.Production)
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.NotAllowedInProductionMode, null);
        }

        return await HandleRenameAsync(path, renameModel.Name, userKey);
    }
}
