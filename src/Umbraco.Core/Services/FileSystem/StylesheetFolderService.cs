using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

internal class StylesheetFolderService : FolderServiceOperationBase<IStylesheetRepository, StylesheetFolderModel, StylesheetFolderOperationStatus>, IStylesheetFolderService
{
    public StylesheetFolderService(IStylesheetRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    protected override StylesheetFolderOperationStatus Success => StylesheetFolderOperationStatus.Success;

    protected override StylesheetFolderOperationStatus NotFound => StylesheetFolderOperationStatus.NotFound;

    protected override StylesheetFolderOperationStatus NotEmpty => StylesheetFolderOperationStatus.NotEmpty;

    protected override StylesheetFolderOperationStatus AlreadyExists => StylesheetFolderOperationStatus.AlreadyExists;

    protected override StylesheetFolderOperationStatus ParentNotFound => StylesheetFolderOperationStatus.ParentNotFound;

    protected override StylesheetFolderOperationStatus InvalidName => StylesheetFolderOperationStatus.InvalidName;


    public async Task<StylesheetFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    public async Task<Attempt<StylesheetFolderModel?, StylesheetFolderOperationStatus>> CreateAsync(StylesheetFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    public async Task<StylesheetFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
