using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IMediaType" />
/// </summary>
internal class MediaTypeRepository : ContentTypeRepositoryBase<IMediaType>, IMediaTypeRepository
{
    public MediaTypeRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MediaTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IShortStringHelper shortStringHelper)
        : base(scopeAccessor, cache, logger, commonRepository, languageRepository, shortStringHelper)
    {
    }

    protected override bool SupportsPublishing => MediaType.SupportsPublishingConst;

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.MediaType;

    protected override IRepositoryCachePolicy<IMediaType, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<IMediaType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);

    // every GetExists method goes cachePolicy.GetSomething which in turns goes PerformGetAll,
    // since this is a FullDataSet policy - and everything is cached
    // so here,
    // every PerformGet/Exists just GetMany() and then filters
    // except PerformGetAll which is the one really doing the job
    protected override IMediaType? PerformGet(int id)
        => GetMany().FirstOrDefault(x => x.Id == id);

    protected override IMediaType? PerformGet(Guid id)
        => GetMany().FirstOrDefault(x => x.Key == id);

    protected override bool PerformExists(Guid id)
        => GetMany().FirstOrDefault(x => x.Key == id) != null;

    protected override IMediaType? PerformGet(string alias)
        => GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

    protected override IEnumerable<IMediaType>? GetAllWithFullCachePolicy() =>
        CommonRepository.GetAllTypes()?.OfType<IMediaType>();

    protected override IEnumerable<IMediaType> PerformGetAll(params Guid[]? ids)
    {
        IEnumerable<IMediaType> all = GetMany();
        return ids?.Any() ?? false ? all.Where(x => ids.Contains(x.Key)) : all;
    }

    protected override IEnumerable<IMediaType> PerformGetByQuery(IQuery<IMediaType> query)
    {
        Sql<ISqlContext> baseQuery = GetBaseQuery(false);
        var translator = new SqlTranslator<IMediaType>(baseQuery, query);
        Sql<ISqlContext> sql = translator.Translate();
        var ids = Database.Fetch<int>(sql).Distinct().ToArray();

        return ids.Length > 0 ? GetMany(ids).OrderBy(x => x.Name).WhereNotNull() : Enumerable.Empty<IMediaType>();
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<ContentTypeDto>(x => x.NodeId);

        sql
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Node}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var l = (List<string>)base.GetDeleteClauses(); // we know it's a list
        l.Add("DELETE FROM cmsContentType WHERE nodeId = @id");
        l.Add("DELETE FROM umbracoNode WHERE id = @id");
        return l;
    }

    protected override void PersistNewItem(IMediaType entity)
    {
        entity.AddingEntity();

        PersistNewBaseContentType(entity);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IMediaType entity)
    {
        ValidateAlias(entity);

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            NodeDto? parent = Database.First<NodeDto>("WHERE id = @ParentId", new { entity.ParentId });
            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;
            var maxSortOrder =
                Database.ExecuteScalar<int>(
                    "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                    new { entity.ParentId, NodeObjectType = NodeObjectTypeId });
            entity.SortOrder = maxSortOrder + 1;
        }

        PersistUpdatedBaseContentType(entity);

        entity.ResetDirtyProperties();
    }
}
