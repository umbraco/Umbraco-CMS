using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Provides navigation services for element items, including querying and managing
///     the element navigation structure and its recycle bin.
/// </summary>
/// <remarks>
///     This service extends <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/>
///     and implements both <see cref="IElementNavigationQueryService"/> and <see cref="IElementNavigationManagementService"/>
///     to provide a complete set of navigation operations for element content.
///     The element tree includes both elements and element containers (folders),
///     so the rebuild queries both object types to build the full tree hierarchy.
/// </remarks>
internal sealed class ElementNavigationService :
    ContentNavigationServiceBase<IContentType, IContentTypeService>,
    IElementNavigationQueryService,
    IElementNavigationManagementService
{
    private static readonly Guid[] ElementObjectTypes =
    [
        Constants.ObjectTypes.Element,
        Constants.ObjectTypes.ElementContainer,
    ];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementNavigationService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider for database operations.</param>
    /// <param name="navigationRepository">The repository for accessing navigation data.</param>
    /// <param name="contentTypeService">The content type service for retrieving content type information.</param>
    public ElementNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository, IContentTypeService contentTypeService)
        : base(coreScopeProvider, navigationRepository, contentTypeService)
    {
    }

    /// <inheritdoc />
    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.ElementTree, ElementObjectTypes, false);

    /// <inheritdoc />
    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.ElementTree, ElementObjectTypes, true);
}
