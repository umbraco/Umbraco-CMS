using NPoco;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories;

public interface IEntityRepositoryExtended : IEntityRepository
{
    /// <summary>
    ///     Gets paged entities for a query and a subset of object types
    /// </summary>
    /// <param name="query"></param>
    /// <param name="objectTypes"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="filter"></param>
    /// <param name="ordering"></param>
    /// <param name="sqlCustomization">
    ///     A callback providing the ability to customize the generated SQL used to retrieve entities
    /// </param>
    /// <returns>
    ///     A collection of mixed entity types which would be of type <see cref="IEntitySlim" />,
    ///     <see cref="IDocumentEntitySlim" />, <see cref="IMediaEntitySlim" />,
    ///     <see cref="IMemberEntitySlim" />
    /// </returns>
    IEnumerable<IEntitySlim> GetPagedResultsByQuery(
        IQuery<IUmbracoEntity> query, Guid[] objectTypes, long pageIndex, int pageSize, out long totalRecords, IQuery<IUmbracoEntity>? filter, Ordering? ordering, Action<Sql<ISqlContext>>? sqlCustomization = null);
}
