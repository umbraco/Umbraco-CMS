using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides navigation services for document (content) items, including querying and managing
///     the document navigation structure and its recycle bin.
/// </summary>
/// <remarks>
///     This service extends <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/>
///     and implements both <see cref="IDocumentNavigationQueryService"/> and <see cref="IDocumentNavigationManagementService"/>
///     to provide a complete set of navigation operations for document content.
/// </remarks>
internal sealed class DocumentNavigationService : ContentNavigationServiceBase<IContentType, IContentTypeService>, IDocumentNavigationQueryService, IDocumentNavigationManagementService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentNavigationService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider for database operations.</param>
    /// <param name="navigationRepository">The repository for accessing navigation data.</param>
    /// <param name="contentTypeService">The content type service for retrieving content type information.</param>
    public DocumentNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository, IContentTypeService contentTypeService)
        : base(coreScopeProvider, navigationRepository, contentTypeService)
    {
    }

    /// <inheritdoc />
    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.ContentTree, Constants.ObjectTypes.Document, false);

    /// <inheritdoc />
    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.ContentTree, Constants.ObjectTypes.Document, true);
}
