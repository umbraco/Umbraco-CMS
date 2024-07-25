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

    public Dictionary<Guid, NavigationNode> GetContentNodesByObjectType(Guid objectTypeKey)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select<NavigationDto>()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == objectTypeKey && x.Trashed == false)
            .OrderBy<NodeDto>(x => x.Path); // make sure that we get the parent items first

        IEnumerable<NavigationDto> navigationDtos = AmbientScope?.Database.Fetch<NavigationDto>(sql) ?? Enumerable.Empty<NavigationDto>();

        //var navigationStructure = NavigationFactory.BuildNavigationDictionary(navigationDtos);
        var contentNavigationStructure = NavigationFactory.BuildNavigationDictionary(navigationDtos, dto => dto.Trashed is false);
        //var recycleBinContentNavigationStructure = NavigationFactory.BuildNavigationDictionary(navigationDtos, dto => dto.Trashed);

        return contentNavigationStructure;
    }

    public Dictionary<Guid, NavigationNode> GetTrashedContentNodesByObjectType(Guid objectTypeKey)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql()
            .Select<NavigationDto>()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == objectTypeKey && x.Trashed == true)
            .OrderBy<NodeDto>(x => x.Path); // make sure that we get the parent items first

        IEnumerable<NavigationDto> navigationDtos = AmbientScope?.Database.Fetch<NavigationDto>(sql) ?? Enumerable.Empty<NavigationDto>();

        return NavigationFactory.BuildNavigationDictionary(navigationDtos);
    }
}
