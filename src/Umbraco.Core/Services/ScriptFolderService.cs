using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

public class ScriptFolderService : PathFolderServiceBase<IScriptRepository>, IScriptFolderService
{
    private readonly IScriptRepository _scriptRepository;

    public ScriptFolderService(IScriptRepository scriptRepository)
    {
        _scriptRepository = scriptRepository;
    }

    public override IScriptRepository Repository => _scriptRepository;
}
