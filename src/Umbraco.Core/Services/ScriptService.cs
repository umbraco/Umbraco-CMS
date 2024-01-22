using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ScriptService : FileServiceOperationBase<IScriptRepository, IScript, ScriptOperationStatus>, IScriptService
{
    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository repository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory, repository, logger, userIdKeyResolver, auditRepository)
    {
    }

    protected override string[] AllowedFileExtensions { get; } = { ".js" };

    protected override ScriptOperationStatus Success => ScriptOperationStatus.Success;

    protected override ScriptOperationStatus NotFound => ScriptOperationStatus.NotFound;

    protected override ScriptOperationStatus CancelledByNotification => ScriptOperationStatus.CancelledByNotification;

    protected override ScriptOperationStatus PathTooLong => ScriptOperationStatus.PathTooLong;

    protected override ScriptOperationStatus AlreadyExists => ScriptOperationStatus.AlreadyExists;

    protected override ScriptOperationStatus ParentNotFound => ScriptOperationStatus.ParentNotFound;

    protected override ScriptOperationStatus InvalidName => ScriptOperationStatus.InvalidName;

    protected override ScriptOperationStatus InvalidFileExtension => ScriptOperationStatus.InvalidFileExtension;

    protected override string EntityType => "Script";

    protected override ScriptSavingNotification SavingNotification(IScript target, EventMessages messages)
        => new(target, messages);

    protected override ScriptSavedNotification SavedNotification(IScript target, EventMessages messages)
        => new(target, messages);

    protected override ScriptDeletingNotification DeletingNotification(IScript target, EventMessages messages)
        => new(target, messages);

    protected override ScriptDeletedNotification DeletedNotification(IScript target, EventMessages messages)
        => new(target, messages);

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
