using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ScriptService : RepositoryService, IScriptService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository scriptRepository,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _scriptRepository = scriptRepository;
        _auditRepository = auditRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    public async Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        string filePath = createModel.ParentPath is null
            ? createModel.Name
            : $"{createModel.ParentPath}/{createModel.Name}";

        if (_scriptRepository.Exists(filePath))
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.AlreadyExists, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var script = new Script(filePath) { Content = createModel.Content };

        var savingNotification = new ScriptSavingNotification(script, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.CancelledByEvent, null);
        }

        _scriptRepository.Save(script);

        scope.Notifications.Publish(new ScriptSavedNotification(script, eventMessages).WithStateFrom(savingNotification));
        int? userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Audit(AuditType.Save, userId ?? -1);

        scope.Complete();
        return Attempt.SucceedWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.Success, script);
    }

    private void Audit(AuditType type, int userId)
    {
        _auditRepository.Save(new AuditItem(-1, type, userId, "Script"));
    }
}
