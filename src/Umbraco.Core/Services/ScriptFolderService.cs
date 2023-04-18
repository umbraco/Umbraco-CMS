using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class ScriptFolderService : PathFolderServiceBase<IScriptRepository, ScriptOperationStatus>, IScriptFolderService
{
    private readonly IScriptRepository _scriptRepository;

    public ScriptFolderService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IScriptRepository scriptRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _scriptRepository = scriptRepository;
    }

    protected override IScriptRepository Repository => _scriptRepository;

    protected override ScriptOperationStatus SuccessStatus => ScriptOperationStatus.Success;

    protected override Task<Attempt<ScriptOperationStatus>> ValidateCreate(PathContainer container)
    {
        if(_scriptRepository.FolderExists(container.Path))
        {
            return Task.FromResult(Attempt.Fail(ScriptOperationStatus.FolderAlreadyExists));
        }

        if (string.IsNullOrWhiteSpace(container.ParentPath) is false &&
            _scriptRepository.FolderExists(container.ParentPath) is false)
        {
            return Task.FromResult(Attempt.Fail(ScriptOperationStatus.ParentNotFound));
        }

        return Task.FromResult(Attempt.Succeed(ScriptOperationStatus.Success));
    }

    protected override Task<Attempt<ScriptOperationStatus>> ValidateDelete(string path)
    {
        if(_scriptRepository.FolderExists(path) is false)
        {
            return Task.FromResult(Attempt.Fail(ScriptOperationStatus.FolderNotFound));
        }

        if (_scriptRepository.FolderHasContent(path))
        {
            return Task.FromResult(Attempt.Fail(ScriptOperationStatus.FolderNotEmpty));
        }

        return Task.FromResult(Attempt.Succeed(ScriptOperationStatus.Success));
    }
}
