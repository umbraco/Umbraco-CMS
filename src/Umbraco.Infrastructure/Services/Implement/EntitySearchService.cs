using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class EntitySearchService : IEntitySearchService
{
    private readonly IEntityService _entityService;
    private readonly ISqlContext _sqlContext;

    public EntitySearchService(IEntityService entityService, ISqlContext sqlContext)
    {
        _entityService = entityService;
        _sqlContext = sqlContext;
    }

    public PagedModel<IEntitySlim> Search(UmbracoObjectTypes objectType, string query, int skip = 0, int take = 100)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize);

        // if the query is a GUID, search for that explicitly
        Guid.TryParse(query, out Guid guidQuery);

        IEntitySlim[] entities = _entityService
            .GetPagedDescendants(
                objectType,
                pageNumber,
                pageSize,
                out long totalRecords,
                _sqlContext.Query<IUmbracoEntity>()
                    .Where(x => x.Name!.Contains(query) || x.Key == guidQuery),
                Ordering.By(nameof(NodeDto.Text).ToFirstLowerInvariant()))
            .ToArray();

        return new PagedModel<IEntitySlim>
        {
            Items = entities,
            Total = totalRecords
        };
    }
}
