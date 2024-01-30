using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class StylesheetService : FileServiceOperationBase<IStylesheetRepository, IStylesheet, StylesheetOperationStatus>, IStylesheetService
{
    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditRepository)
    {
    }

    protected override string[] AllowedFileExtensions { get; } = { ".css" };

    protected override StylesheetOperationStatus Success => StylesheetOperationStatus.Success;

    protected override StylesheetOperationStatus NotFound => StylesheetOperationStatus.NotFound;

    protected override StylesheetOperationStatus CancelledByNotification => StylesheetOperationStatus.CancelledByNotification;

    protected override StylesheetOperationStatus PathTooLong => StylesheetOperationStatus.PathTooLong;

    protected override StylesheetOperationStatus AlreadyExists => StylesheetOperationStatus.AlreadyExists;

    protected override StylesheetOperationStatus ParentNotFound => StylesheetOperationStatus.ParentNotFound;

    protected override StylesheetOperationStatus InvalidName => StylesheetOperationStatus.InvalidName;

    protected override StylesheetOperationStatus InvalidFileExtension => StylesheetOperationStatus.InvalidFileExtension;

    protected override string EntityType => "Stylesheet";

    protected override StylesheetSavingNotification SavingNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    protected override StylesheetSavedNotification SavedNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    protected override StylesheetDeletingNotification DeletingNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

    protected override StylesheetDeletedNotification DeletedNotification(IStylesheet target, EventMessages messages)
        => new(target, messages);

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
