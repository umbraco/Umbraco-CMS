using System.Collections.Concurrent;
using NPoco;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
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
    public ConcurrentDictionary<Guid, NavigationNode> GetContentNodesByObjectType(Guid objectTypeKey)
    {
        IEnumerable<NavigationDto> navigationDtos = FetchNavigationDtos(objectTypeKey, false);
        return NavigationFactory.BuildNavigationDictionary(navigationDtos);
    }

    /// <inheritdoc />
    public ConcurrentDictionary<Guid, NavigationNode> GetTrashedContentNodesByObjectType(Guid objectTypeKey)
    {
        IEnumerable<NavigationDto> navigationDtos = FetchNavigationDtos(objectTypeKey, true);
        return NavigationFactory.BuildNavigationDictionary(navigationDtos);
    }

    private IEnumerable<NavigationDto> FetchNavigationDtos(Guid objectTypeKey, bool trashed)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select<NavigationDto>()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == objectTypeKey && x.Trashed == trashed)
            .OrderBy<NodeDto>(x => x.Path); // make sure that we get the parent items first

        return AmbientScope?.Database.Fetch<NavigationDto>(sql) ?? Enumerable.Empty<NavigationDto>();
    }
}
