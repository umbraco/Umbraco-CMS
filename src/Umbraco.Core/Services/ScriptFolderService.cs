using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class ScriptFolderService : PathFolderServiceBase<IScriptRepository, ScriptFolderOperationStatus>, IScriptFolderService
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

    protected override ScriptFolderOperationStatus SuccessStatus => ScriptFolderOperationStatus.Success;

    protected override Task<Attempt<ScriptFolderOperationStatus>> ValidateCreateAsync(PathContainer container)
    {
        if (container.Name.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(Attempt.Fail(ScriptFolderOperationStatus.InvalidName));
        }

        if (_scriptRepository.FolderExists(container.Path))
        {
            return Task.FromResult(Attempt.Fail(ScriptFolderOperationStatus.AlreadyExists));
        }

        if (string.IsNullOrWhiteSpace(container.ParentPath) is false &&
            _scriptRepository.FolderExists(container.ParentPath) is false)
        {
            return Task.FromResult(Attempt.Fail(ScriptFolderOperationStatus.ParentNotFound));
        }

        return Task.FromResult(Attempt.Succeed(ScriptFolderOperationStatus.Success));
    }

    protected override Task<Attempt<ScriptFolderOperationStatus>> ValidateDeleteAsync(string path)
    {
        if(_scriptRepository.FolderExists(path) is false)
        {
            return Task.FromResult(Attempt.Fail(ScriptFolderOperationStatus.NotFound));
        }

        if (_scriptRepository.FolderHasContent(path))
        {
            return Task.FromResult(Attempt.Fail(ScriptFolderOperationStatus.NotEmpty));
        }

        return Task.FromResult(Attempt.Succeed(ScriptFolderOperationStatus.Success));
    }
}
