using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ScriptService : FileServiceBase, IScriptService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<ScriptService> _logger;

    private readonly string[] _allowedFileExtensions = { ".js" };

    protected override string[] GetAllowedFileExtensions() => _allowedFileExtensions;

    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository scriptRepository,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver,
        ILogger<ScriptService> logger)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _scriptRepository = scriptRepository;
        _auditRepository = auditRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _logger = logger;
    }

    public Task<IScript?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IScript? script = _scriptRepository.Get(path);

        scope.Complete();
        return Task.FromResult(script);
    }

    public async Task<ScriptOperationStatus> DeleteAsync(string path, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IScript? script = _scriptRepository.Get(path);
        if (script is null)
        {
            return ScriptOperationStatus.NotFound;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingNotification = new ScriptDeletingNotification(script, eventMessages);
        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            return ScriptOperationStatus.CancelledByNotification;
        }

        _scriptRepository.Delete(script);

        scope.Notifications.Publish(
            new ScriptDeletedNotification(script, eventMessages).WithStateFrom(deletingNotification));

        int performingUserId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Audit(AuditType.Delete, performingUserId);

        scope.Complete();
        return ScriptOperationStatus.Success;
    }

    public async Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        try
        {
            ScriptOperationStatus validationResult = await ValidateCreateAsync(createModel);
            if (validationResult is not ScriptOperationStatus.Success)
            {
                return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(validationResult, null);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The script path was too long");
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.PathTooLong, null);
        }

        var script = new Script(createModel.FilePath) { Content = createModel.Content };

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new ScriptSavingNotification(script, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.CancelledByNotification, null);
        }

        _scriptRepository.Save(script);

        scope.Notifications.Publish(new ScriptSavedNotification(script, eventMessages).WithStateFrom(savingNotification));
        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Audit(AuditType.Save, userId);

        scope.Complete();
        return Attempt.SucceedWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.Success, script);
    }

    private Task<ScriptOperationStatus> ValidateCreateAsync(ScriptCreateModel createModel)
    {
        if (_scriptRepository.Exists(createModel.FilePath))
        {
            return Task.FromResult(ScriptOperationStatus.AlreadyExists);
        }

        if (string.IsNullOrWhiteSpace(createModel.ParentPath) is false &&
            _scriptRepository.FolderExists(createModel.ParentPath) is false)
        {
            return Task.FromResult(ScriptOperationStatus.ParentNotFound);
        }

        if(HasValidFileName(createModel.Name) is false)
        {
            return Task.FromResult(ScriptOperationStatus.InvalidName);
        }

        if (HasValidFileExtension(createModel.FilePath) is false)
        {
            return Task.FromResult(ScriptOperationStatus.InvalidFileExtension);
        }

        return Task.FromResult(ScriptOperationStatus.Success);
    }

    public async Task<Attempt<IScript?, ScriptOperationStatus>> UpdateAsync(ScriptUpdateModel updateModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IScript? script = _scriptRepository.Get(updateModel.ExistingPath);

        if (script is null)
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.NotFound, null);
        }

        ScriptOperationStatus validationResult = ValidateUpdate(updateModel);
        if (validationResult is not ScriptOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(validationResult, null);
        }

        script.Content = updateModel.Content;
        if (script.Name != updateModel.Name)
        {
            // Name has been updated, so we need to update the path as well
            var newPath = script.Path.Replace(script.Name!, updateModel.Name);
            script.Path = newPath;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new ScriptSavingNotification(script, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.CancelledByNotification, null);
        }

        _scriptRepository.Save(script);
        scope.Notifications.Publish(new ScriptSavedNotification(script, eventMessages).WithStateFrom(savingNotification));

        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Audit(AuditType.Save, userId);

        scope.Complete();
        return Attempt.SucceedWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.Success, script);
    }

    private ScriptOperationStatus ValidateUpdate(ScriptUpdateModel updateModel)
    {
        if (HasValidFileExtension(updateModel.Name) is false)
        {
            return ScriptOperationStatus.InvalidFileExtension;
        }

        if (HasValidFileName(updateModel.Name) is false)
        {
            return ScriptOperationStatus.InvalidName;
        }

        return ScriptOperationStatus.Success;
    }

    private void Audit(AuditType type, int userId)
        => _auditRepository.Save(new AuditItem(-1, type, userId, "Script"));
}
