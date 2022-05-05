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
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IContentType" />
/// </summary>
internal class ContentTypeRepository : ContentTypeRepositoryBase<IContentType>, IContentTypeRepository
{
    public ContentTypeRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<ContentTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IShortStringHelper shortStringHelper)
        : base(scopeAccessor, cache, logger, commonRepository, languageRepository, shortStringHelper)
    {
    }

    protected override bool SupportsPublishing => ContentType.SupportsPublishingConst;

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentType;

    /// <inheritdoc />
    public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
    {
        var ints = PerformGetByQuery(query).ToArray();
        return ints.Length > 0 ? GetMany(ints) : Enumerable.Empty<IContentType>();
    }

    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllPropertyTypeAliases() =>
        Database.Fetch<string>("SELECT DISTINCT Alias FROM cmsPropertyType ORDER BY Alias");

    /// <summary>
    ///     Gets all content type aliases
    /// </summary>
    /// <param name="objectTypes">
    ///     If this list is empty, it will return all content type aliases for media, members and content, otherwise
    ///     it will only return content type aliases for the object types specified
    /// </param>
    /// <returns></returns>
    public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
    {
        Sql<ISqlContext> sql = Sql()
            .Select("cmsContentType.alias")
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>()
            .On<ContentTypeDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId);

        if (objectTypes.Any())
        {
            sql = sql.WhereIn<NodeDto>(dto => dto.NodeObjectType, objectTypes);
        }

        return Database.Fetch<string>(sql);
    }

    public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
    {
        if (aliases.Length == 0)
        {
            return Enumerable.Empty<int>();
        }

        Sql<ISqlContext> sql = Sql()
            .Select<ContentTypeDto>(x => x.NodeId)
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>()
            .On<ContentTypeDto, NodeDto>(dto => dto.NodeId, dto => dto.NodeId)
            .Where<ContentTypeDto>(dto => aliases.Contains(dto.Alias));

        return Database.Fetch<int>(sql);
    }

    protected override IRepositoryCachePolicy<IContentType, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<IContentType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);

    // every GetExists method goes cachePolicy.GetSomething which in turns goes PerformGetAll,
    // since this is a FullDataSet policy - and everything is cached
    // so here,
    // every PerformGet/Exists just GetMany() and then filters
    // except PerformGetAll which is the one really doing the job

    // TODO: the filtering is highly inefficient as we deep-clone everything
    // there should be a way to GetMany(predicate) right from the cache policy!
    // and ah, well, this all caching should be refactored + the cache refreshers
    // should to repository.Clear() not deal with magic caches by themselves
    protected override IContentType? PerformGet(int id)
        => GetMany().FirstOrDefault(x => x.Id == id);

    protected override IContentType? PerformGet(Guid id)
        => GetMany().FirstOrDefault(x => x.Key == id);

    protected override IContentType? PerformGet(string alias)
        => GetMany().FirstOrDefault(x => x.Alias.InvariantEquals(alias));

    protected override bool PerformExists(Guid id)
        => GetMany().FirstOrDefault(x => x.Key == id) != null;

    protected override IEnumerable<IContentType>? GetAllWithFullCachePolicy() =>
        CommonRepository.GetAllTypes()?.OfType<IContentType>();

    protected override IEnumerable<IContentType> PerformGetAll(params Guid[]? ids)
    {
        IEnumerable<IContentType> all = GetMany();
        return ids?.Any() ?? false ? all.Where(x => ids.Contains(x.Key)) : all;
    }

    protected override IEnumerable<IContentType> PerformGetByQuery(IQuery<IContentType> query)
    {
        Sql<ISqlContext> baseQuery = GetBaseQuery(false);
        var translator = new SqlTranslator<IContentType>(baseQuery, query);
        Sql<ISqlContext> sql = translator.Translate();
        var ids = Database.Fetch<int>(sql).Distinct().ToArray();

        return ids.Length > 0
            ? GetMany(ids).OrderBy(x => x.Name)
            : Enumerable.Empty<IContentType>();
    }

    protected IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
    {
        // used by DataTypeService to remove properties
        // from content types if they have a deleted data type - see
        // notes in DataTypeService.Delete as it's a bit weird
        Sql<ISqlContext> sqlClause = Sql()
            .SelectAll()
            .From<PropertyTypeDto>()
            .LeftJoin<PropertyTypeGroupDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

        var translator = new SqlTranslator<PropertyType>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate()
            .OrderBy<PropertyTypeDto>(x => x.PropertyTypeGroupId);

        return Database
            .FetchOneToMany<PropertyTypeGroupDto>(x => x.PropertyTypeDtos, sql)
            .Select(x => x.ContentTypeNodeId).Distinct();
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
            .LeftJoin<ContentTypeTemplateDto>()
            .On<ContentTypeTemplateDto, ContentTypeDto>(left => left.ContentTypeNodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Node}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var l = (List<string>)base.GetDeleteClauses(); // we know it's a list
        l.Add("DELETE FROM cmsDocumentType WHERE contentTypeNodeId = @id");
        l.Add("DELETE FROM cmsContentType WHERE nodeId = @id");
        l.Add("DELETE FROM umbracoNode WHERE id = @id");
        return l;
    }

    /// <summary>
    ///     Deletes a content type
    /// </summary>
    /// <param name="entity"></param>
    /// <remarks>
    ///     First checks for children and removes those first
    /// </remarks>
    protected override void PersistDeletedItem(IContentType entity)
    {
        IQuery<IContentType> query = Query<IContentType>().Where(x => x.ParentId == entity.Id);
        IEnumerable<IContentType> children = Get(query);
        foreach (IContentType child in children)
        {
            PersistDeletedItem(child);
        }

        // Before we call the base class methods to run all delete clauses, we need to first
        // delete all of the property data associated with this document type. Normally this will
        // be done in the ContentTypeService by deleting all associated content first, but in some cases
        // like when we switch a document type, there is property data left over that is linked
        // to the previous document type. So we need to ensure it's removed.
        Sql<ISqlContext> sql = Sql()
            .Select("DISTINCT " + Constants.DatabaseSchema.Tables.PropertyData + ".propertytypeid")
            .From<PropertyDataDto>()
            .InnerJoin<PropertyTypeDto>()
            .On<PropertyDataDto, PropertyTypeDto>(dto => dto.PropertyTypeId, dto => dto.Id)
            .InnerJoin<ContentTypeDto>()
            .On<ContentTypeDto, PropertyTypeDto>(dto => dto.NodeId, dto => dto.ContentTypeId)
            .Where<ContentTypeDto>(dto => dto.NodeId == entity.Id);

        // Delete all PropertyData where propertytypeid EXISTS in the subquery above
        Database.Execute(SqlSyntax.GetDeleteSubquery(Constants.DatabaseSchema.Tables.PropertyData, "propertytypeid", sql));

        base.PersistDeletedItem(entity);
    }

    protected override void PersistNewItem(IContentType entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Alias))
        {
            var ex = new Exception(
                $"ContentType '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");
            Logger.LogError(
                "ContentType '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                entity.Name);
            throw ex;
        }

        entity.AddingEntity();

        PersistNewBaseContentType(entity);
        PersistTemplates(entity, false);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    protected void PersistTemplates(IContentType entity, bool clearAll)
    {
        // remove and insert, if required
        Database.Delete<ContentTypeTemplateDto>("WHERE contentTypeNodeId = @Id", new { entity.Id });

        // we could do it all in foreach if we assume that the default template is an allowed template??
        var defaultTemplateId = entity.DefaultTemplateId;
        if (defaultTemplateId > 0)
        {
            Database.Insert(new ContentTypeTemplateDto
            {
                ContentTypeNodeId = entity.Id, TemplateNodeId = defaultTemplateId, IsDefault = true,
            });
        }

        foreach (ITemplate template in entity.AllowedTemplates?.Where(x => x.Id != defaultTemplateId) ??
                                       Array.Empty<ITemplate>())
        {
            Database.Insert(new ContentTypeTemplateDto
            {
                ContentTypeNodeId = entity.Id, TemplateNodeId = template.Id, IsDefault = false,
            });
        }
    }

    protected override void PersistUpdatedItem(IContentType entity)
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
        PersistTemplates(entity, true);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    private void PersistHistoryCleanup(IContentType entity)
    {
        // historyCleanup property is not mandatory for api endpoint, handle the case where it's not present.
        // DocumentTypeSave doesn't handle this for us like ContentType constructors do.
        if (entity is IContentTypeWithHistoryCleanup entityWithHistoryCleanup)
        {
            var dto = new ContentVersionCleanupPolicyDto
            {
                ContentTypeId = entity.Id,
                Updated = DateTime.Now,
                PreventCleanup = entityWithHistoryCleanup.HistoryCleanup?.PreventCleanup ?? false,
                KeepAllVersionsNewerThanDays =
                    entityWithHistoryCleanup.HistoryCleanup?.KeepAllVersionsNewerThanDays,
                KeepLatestVersionPerDayForDays =
                    entityWithHistoryCleanup.HistoryCleanup?.KeepLatestVersionPerDayForDays,
            };
            Database.InsertOrUpdate(dto);
        }
    }
}
