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
            .Select(
                $"n.{NodeDto.IdColumnName} as {NodeDto.IdColumnName}",
                $"n.{NodeDto.KeyColumnName} as {NodeDto.KeyColumnName}",
                $"ctn.{NodeDto.KeyColumnName} as {NavigationDto.ContentTypeKeyColumnName}",
                $"n.{NodeDto.ParentIdColumnName} as {NodeDto.ParentIdColumnName}",
                $"n.{NodeDto.SortOrderColumnName} as {NodeDto.SortOrderColumnName}",
                $"n.{NodeDto.TrashedColumnName} as {NodeDto.TrashedColumnName}")
            .From<NodeDto>("n")
            .InnerJoin<ContentDto>("c").On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId, "n", "c")
            .InnerJoin<NodeDto>("ctn").On<ContentDto, NodeDto>((c, ctn) => c.ContentTypeId == ctn.NodeId, "c", "ctn")
            .Where<NodeDto>(n => n.NodeObjectType == objectTypeKey && n.Trashed == trashed, "n")
            .OrderBy<NodeDto>(n => n.Path, "n"); // make sure that we get the parent items first

        return AmbientScope?.Database.Fetch<NavigationDto>(sql) ?? Enumerable.Empty<NavigationDto>();
    }
}
