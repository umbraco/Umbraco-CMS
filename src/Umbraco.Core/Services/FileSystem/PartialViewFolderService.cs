using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

internal class PartialViewFolderService : FolderServiceOperationBase<IPartialViewRepository, PartialViewFolderModel, PartialViewFolderOperationStatus>, IPartialViewFolderService
{
    public PartialViewFolderService(IPartialViewRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    protected override PartialViewFolderOperationStatus Success => PartialViewFolderOperationStatus.Success;

    protected override PartialViewFolderOperationStatus NotFound => PartialViewFolderOperationStatus.NotFound;

    protected override PartialViewFolderOperationStatus NotEmpty => PartialViewFolderOperationStatus.NotEmpty;

    protected override PartialViewFolderOperationStatus AlreadyExists => PartialViewFolderOperationStatus.AlreadyExists;

    protected override PartialViewFolderOperationStatus ParentNotFound => PartialViewFolderOperationStatus.ParentNotFound;

    protected override PartialViewFolderOperationStatus InvalidName => PartialViewFolderOperationStatus.InvalidName;


    public async Task<PartialViewFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    public async Task<Attempt<PartialViewFolderModel?, PartialViewFolderOperationStatus>> CreateAsync(PartialViewFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    public async Task<PartialViewFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
