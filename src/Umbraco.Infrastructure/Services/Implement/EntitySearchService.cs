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

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Services.Implement.EntitySearchService"/> class.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities.</param>
    /// <param name="sqlContext">The SQL context used for database operations.</param>
    public EntitySearchService(IEntityService entityService, ISqlContext sqlContext)
    {
        _entityService = entityService;
        _sqlContext = sqlContext;
    }

    /// <summary>
    /// Searches for entities of the specified Umbraco object type whose names contain the given query string or whose key matches the query if it is a GUID.
    /// </summary>
    /// <param name="objectType">The type of Umbraco object to search for.</param>
    /// <param name="query">The search query string. Matches entities whose name contains this string, or whose key matches if the query is a GUID.</param>
    /// <param name="skip">The number of items to skip for paging. Defaults to 0.</param>
    /// <param name="take">The number of items to return for paging. Defaults to 100.</param>
    /// <returns>
    /// A <see cref="PagedModel{IEntitySlim}"/> containing the matching entities and the total number of matches.
    /// </returns>
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
