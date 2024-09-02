using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class ContentNavigationRepository : INavigationRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public ContentNavigationRepository(IScopeAccessor scopeAccessor)
        => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetContentNodesByObjectType(Guid objectTypeKey)
        => FetchNavigationDtos(objectTypeKey, false);

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(Guid objectTypeKey)
        => FetchNavigationDtos(objectTypeKey, true);

    private IEnumerable<INavigationModel> FetchNavigationDtos(Guid objectTypeKey, bool trashed)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select<NavigationDto>()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == objectTypeKey && x.Trashed == trashed)
            .OrderBy<NodeDto>(x => x.Path); // make sure that we get the parent items first

        return AmbientScope?.Database.Fetch<NavigationDto>(sql) ?? Enumerable.Empty<NavigationDto>();
    }
}
