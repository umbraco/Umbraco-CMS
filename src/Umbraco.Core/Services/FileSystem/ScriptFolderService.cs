using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

internal class ScriptFolderService : FolderServiceOperationBase<IScriptRepository, ScriptFolderModel, ScriptFolderOperationStatus>, IScriptFolderService
{
    public ScriptFolderService(IScriptRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    protected override ScriptFolderOperationStatus Success => ScriptFolderOperationStatus.Success;

    protected override ScriptFolderOperationStatus NotFound => ScriptFolderOperationStatus.NotFound;

    protected override ScriptFolderOperationStatus NotEmpty => ScriptFolderOperationStatus.NotEmpty;

    protected override ScriptFolderOperationStatus AlreadyExists => ScriptFolderOperationStatus.AlreadyExists;

    protected override ScriptFolderOperationStatus ParentNotFound => ScriptFolderOperationStatus.ParentNotFound;

    protected override ScriptFolderOperationStatus InvalidName => ScriptFolderOperationStatus.InvalidName;


    public async Task<ScriptFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    public async Task<Attempt<ScriptFolderModel?, ScriptFolderOperationStatus>> CreateAsync(ScriptFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    public async Task<ScriptFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
