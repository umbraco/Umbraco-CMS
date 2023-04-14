using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
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
    private readonly IUmbracoMapper _mapper;
    private readonly ILogger<ScriptService> _logger;

    private readonly string[] _allowedFileExtensions = { ".js" };

    public ScriptService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository scriptRepository,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IUmbracoMapper mapper,
        ILogger<ScriptService> logger)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _scriptRepository = scriptRepository;
        _auditRepository = auditRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<IScript?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IScript? script = _scriptRepository.Get(path);

        scope.Complete();
        return Task.FromResult(script);
    }

    public async Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        try
        {
            ScriptOperationStatus validationResult = ValidateSave(createModel);
            if (validationResult is not ScriptOperationStatus.Success)
            {
                return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(validationResult, null);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The script path is too long");
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.PathTooLong, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var script = new Script(createModel.FilePath) { Content = createModel.Content };

        var savingNotification = new ScriptSavingNotification(script, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.CancelledByNotification, null);
        }

        _scriptRepository.Save(script);

        scope.Notifications.Publish(new ScriptSavedNotification(script, eventMessages).WithStateFrom(savingNotification));
        int? userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Audit(AuditType.Save, userId ?? -1);

        scope.Complete();
        return Attempt.SucceedWithStatus<IScript?, ScriptOperationStatus>(ScriptOperationStatus.Success, script);
    }

    private ScriptOperationStatus ValidateSave(ScriptCreateModel createModel)
    {
        if (_scriptRepository.Exists(createModel.FilePath))
        {
            return ScriptOperationStatus.AlreadyExists;
        }

        if (string.IsNullOrEmpty(createModel.ParentPath) is false && _scriptRepository.FolderExists(createModel.ParentPath) is false)
        {
            return ScriptOperationStatus.ParentNotFound;
        }

        if (HasValidFileExtension(createModel.FilePath) is false)
        {
            return ScriptOperationStatus.InvalidFileExtension;
        }

        return ScriptOperationStatus.Success;
    }

    private bool HasValidFileExtension(string fileName)
        => _allowedFileExtensions.Contains(Path.GetExtension(fileName));

    private void Audit(AuditType type, int userId)
        => _auditRepository.Save(new AuditItem(-1, type, userId, "Script"));
}
