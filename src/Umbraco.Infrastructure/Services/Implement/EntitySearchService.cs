using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
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

        IQuery<IUmbracoEntity> filter = CreateQueryFilter(query);

        IEntitySlim[] entities = _entityService
            .GetPagedDescendants(
                objectType,
                pageNumber,
                pageSize,
                out long totalRecords,
                filter,
                Ordering.By(nameof(NodeDto.Text).ToFirstLowerInvariant()))
            .ToArray();

        return new PagedModel<IEntitySlim>
        {
            Items = entities,
            Total = totalRecords
        };
    }

    public PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, string query, int skip = 0, int take = 100)
        => SearchInternal(objectTypes, query, skip, take);

    public PagedModel<IEntitySlim> Search(IEnumerable<UmbracoObjectTypes> objectTypes, int skip = 0, int take = 100)
        => SearchInternal(objectTypes, null, skip, take);

    private PagedModel<IEntitySlim> SearchInternal(IEnumerable<UmbracoObjectTypes> objectTypes, string? query, int skip, int take)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out long pageNumber, out int pageSize);

        IQuery<IUmbracoEntity> filter = CreateQueryFilter(query);

        IEntitySlim[] entities = _entityService
            .GetPagedDescendants(
                objectTypes,
                pageNumber,
                pageSize,
                out long totalRecords,
                filter,
                Ordering.By(nameof(NodeDto.Text).ToFirstLowerInvariant()))
            .ToArray();

        return new PagedModel<IEntitySlim>
        {
            Items = entities,
            Total = totalRecords
        };
    }

    private IQuery<IUmbracoEntity> CreateQueryFilter(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _sqlContext.Query<IUmbracoEntity>();
        }

        if (Guid.TryParse(query, out Guid guidQuery))
        {
            return _sqlContext.Query<IUmbracoEntity>().Where(x => x.Key == guidQuery);
        }

        return _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name!.Contains(query));
    }
}
