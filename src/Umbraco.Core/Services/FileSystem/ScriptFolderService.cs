using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Provides a service for managing script folders in the file system.
/// </summary>
/// <remarks>
///     This service handles the creation, retrieval, and deletion of folders within the script
///     file system area. It inherits common folder operation logic from
///     <see cref="FolderServiceOperationBase{TRepository, TFolderModel, TOperationStatus}"/>.
/// </remarks>
internal sealed class ScriptFolderService : FolderServiceOperationBase<IScriptRepository, ScriptFolderModel, ScriptFolderOperationStatus>, IScriptFolderService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptFolderService"/> class.
    /// </summary>
    /// <param name="repository">The script repository for file system operations.</param>
    /// <param name="scopeProvider">The scope provider for database transactions.</param>
    public ScriptFolderService(IScriptRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus Success => ScriptFolderOperationStatus.Success;

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus NotFound => ScriptFolderOperationStatus.NotFound;

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus NotEmpty => ScriptFolderOperationStatus.NotEmpty;

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus AlreadyExists => ScriptFolderOperationStatus.AlreadyExists;

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus ParentNotFound => ScriptFolderOperationStatus.ParentNotFound;

    /// <inheritdoc/>
    protected override ScriptFolderOperationStatus InvalidName => ScriptFolderOperationStatus.InvalidName;


    /// <inheritdoc/>
    public async Task<ScriptFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    /// <inheritdoc/>
    public async Task<Attempt<ScriptFolderModel?, ScriptFolderOperationStatus>> CreateAsync(ScriptFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    /// <inheritdoc/>
    public async Task<ScriptFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
