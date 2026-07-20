using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Provides a service for managing partial view folders in the file system.
/// </summary>
/// <remarks>
///     This service handles the creation, retrieval, and deletion of folders within the partial view
///     file system area. It inherits common folder operation logic from
///     <see cref="FolderServiceOperationBase{TRepository, TFolderModel, TOperationStatus}"/>.
/// </remarks>
internal sealed class PartialViewFolderService : FolderServiceOperationBase<IPartialViewRepository, PartialViewFolderModel, PartialViewFolderOperationStatus>, IPartialViewFolderService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialViewFolderService"/> class.
    /// </summary>
    /// <param name="repository">The partial view repository for file system operations.</param>
    /// <param name="scopeProvider">The scope provider for database transactions.</param>
    public PartialViewFolderService(IPartialViewRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus Success => PartialViewFolderOperationStatus.Success;

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus NotFound => PartialViewFolderOperationStatus.NotFound;

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus NotEmpty => PartialViewFolderOperationStatus.NotEmpty;

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus AlreadyExists => PartialViewFolderOperationStatus.AlreadyExists;

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus ParentNotFound => PartialViewFolderOperationStatus.ParentNotFound;

    /// <inheritdoc/>
    protected override PartialViewFolderOperationStatus InvalidName => PartialViewFolderOperationStatus.InvalidName;


    /// <inheritdoc/>
    public async Task<PartialViewFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    /// <inheritdoc/>
    public async Task<Attempt<PartialViewFolderModel?, PartialViewFolderOperationStatus>> CreateAsync(PartialViewFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    /// <inheritdoc/>
    public async Task<PartialViewFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
