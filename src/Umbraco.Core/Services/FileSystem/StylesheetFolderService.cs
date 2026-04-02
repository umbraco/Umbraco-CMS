using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Provides a service for managing stylesheet folders in the file system.
/// </summary>
/// <remarks>
///     This service handles the creation, retrieval, and deletion of folders within the stylesheet
///     file system area. It inherits common folder operation logic from
///     <see cref="FolderServiceOperationBase{TRepository, TFolderModel, TOperationStatus}"/>.
/// </remarks>
internal sealed class StylesheetFolderService : FolderServiceOperationBase<IStylesheetRepository, StylesheetFolderModel, StylesheetFolderOperationStatus>, IStylesheetFolderService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StylesheetFolderService"/> class.
    /// </summary>
    /// <param name="repository">The stylesheet repository for file system operations.</param>
    /// <param name="scopeProvider">The scope provider for database transactions.</param>
    public StylesheetFolderService(IStylesheetRepository repository, ICoreScopeProvider scopeProvider)
        : base(repository, scopeProvider)
    {
    }

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus Success => StylesheetFolderOperationStatus.Success;

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus NotFound => StylesheetFolderOperationStatus.NotFound;

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus NotEmpty => StylesheetFolderOperationStatus.NotEmpty;

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus AlreadyExists => StylesheetFolderOperationStatus.AlreadyExists;

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus ParentNotFound => StylesheetFolderOperationStatus.ParentNotFound;

    /// <inheritdoc/>
    protected override StylesheetFolderOperationStatus InvalidName => StylesheetFolderOperationStatus.InvalidName;


    /// <inheritdoc/>
    public async Task<StylesheetFolderModel?> GetAsync(string path)
        => await HandleGetAsync(path);

    /// <inheritdoc/>
    public async Task<Attempt<StylesheetFolderModel?, StylesheetFolderOperationStatus>> CreateAsync(StylesheetFolderCreateModel createModel)
        => await HandleCreateAsync(createModel.Name, createModel.ParentPath);

    /// <inheritdoc/>
    public async Task<StylesheetFolderOperationStatus> DeleteAsync(string path) => await HandleDeleteAsync(path);
}
