using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IElement" />.
/// </summary>
// TODO ELEMENTS: refactor and reuse code from DocumentRepository (note there is an NPoco issue with generics, so we have to live with a certain amount of code duplication)
internal class ElementRepository : PublishableContentRepositoryBase<IElement, ElementRepository, ElementDto, ElementVersionDto, ElementCultureVariationDto>, IElementRepository
{
    private readonly AppCaches _appCaches;
    private readonly IContentTypeRepository _contentTypeRepository;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="scopeAccessor"></param>
    /// <param name="appCaches"></param>
    /// <param name="logger"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="contentTypeRepository"></param>
    /// <param name="tagRepository"></param>
    /// <param name="languageRepository"></param>
    /// <param name="relationRepository"></param>
    /// <param name="relationTypeRepository"></param>
    /// <param name="dataValueReferenceFactories"></param>
    /// <param name="dataTypeService"></param>
    /// <param name="serializer"></param>
    /// <param name="eventAggregator"></param>
    /// <param name="propertyEditors">
    ///     Lazy property value collection - must be lazy because we have a circular dependency since some property editors
    ///     require services, yet these services require property editors
    /// </param>
    /// <param name="repositoryCacheVersionService"></param>
    /// <param name="cacheSyncService"></param>
    public ElementRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<ElementRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
                serializer,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _contentTypeRepository =
            contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _appCaches = appCaches;
    }

    protected override ElementRepository This => this;

    /// <summary>
    ///     Default is to always ensure all elements have unique names
    /// </summary>
    protected virtual bool EnsureUniqueNaming { get; } = true;

    protected override IEnumerable<IElement> MapDtosToContent(List<ElementDto> dtos,
        bool withCache = false,
        string[]? propertyAliases = null,
        bool loadTemplates = true,
        bool loadVariants = true)
    {
        var temps = new List<TempContent<IElement>>();
        var contentTypes = new Dictionary<int, IContentType?>();

        var content = new IElement[dtos.Count];

        for (var i = 0; i < dtos.Count; i++)
        {
            ElementDto dto = dtos[i];

            if (withCache)
            {
                // if the cache contains the (proper version of the) item, use it
                IElement? cached =
                    IsolatedCache.GetCacheItem<IElement>(RepositoryCacheKeys.GetKey<IElement, int>(dto.NodeId));
                if (cached != null && cached.VersionId == dto.ContentVersionDto.ContentVersionDto.Id)
                {
                    content[i] = cached;
                    continue;
                }
            }

            // else, need to build it

            // get the content type - the repository is full cache *but* still deep-clones
            // whatever comes out of it, so use our own local index here to avoid this
            var contentTypeId = dto.ContentDto.ContentTypeId;
            if (contentTypes.TryGetValue(contentTypeId, out IContentType? contentType) == false)
            {
                contentTypes[contentTypeId] = contentType = _contentTypeRepository.Get(contentTypeId);
            }

            IElement c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

            // need temps, for properties, templates and variations
            var versionId = dto.ContentVersionDto.Id;
            var publishedVersionId = dto.Published ? dto.PublishedVersionDto!.Id : 0;
            var temp = new TempContent<IElement>(dto.NodeId, versionId, publishedVersionId, contentType, c);

            temps.Add(temp);
        }

        // An empty array of propertyAliases indicates that no properties need to be loaded (null = load all properties).
        var loadProperties = propertyAliases is { Length: 0 } is false;

        IDictionary<int, PropertyCollection>? properties = null;
        if (loadProperties)
        {
            // load all properties for all elements from database in 1 query - indexed by version id
            properties = GetPropertyCollections(temps, propertyAliases);
        }

        // assign templates and properties
        foreach (TempContent<IElement> temp in temps)
        {
            // set properties
            if (loadProperties)
            {
                if (properties?.ContainsKey(temp.VersionId) ?? false)
                {
                    temp.Content!.Properties = properties[temp.VersionId];
                }
                else
                {
                    throw new InvalidOperationException($"No property data found for version: '{temp.VersionId}'.");
                }
            }
            else
            {
                // When loadProperties is false (propertyAliases is empty array), clear the property collection
                temp.Content!.Properties = new PropertyCollection();
            }
        }

        if (loadVariants)
        {
            // set variations, if varying
            temps = temps.Where(x => x.ContentType?.VariesByCulture() ?? false).ToList();
            if (temps.Count > 0)
            {
                // load all variations for all elements from database, in one query
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(temps);
                IDictionary<int, List<EntityVariation>> elementVariations = GetEntityVariations(temps);
                foreach (TempContent<IElement> temp in temps)
                {
                    SetVariations(temp.Content, contentVariations, elementVariations);
                }
            }
        }

        foreach (IElement c in content)
        {
            c.ResetDirtyProperties(false); // reset dirty initial properties (U4-1946)
        }

        return content;
    }

    protected override IElement MapDtoToContent(ElementDto dto)
    {
        IContentType? contentType = _contentTypeRepository.Get(dto.ContentDto.ContentTypeId);
        IElement content = ContentBaseFactory.BuildEntity(dto, contentType);

        try
        {
            content.DisableChangeTracking();

            // get properties - indexed by version id
            var versionId = dto.ContentVersionDto.Id;

            // TODO: shall we get published properties or not?
            //var publishedVersionId = dto.Published ? dto.PublishedVersionDto.Id : 0;
            var publishedVersionId = dto.PublishedVersionDto?.Id ?? 0;

            var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType);
            var ltemp = new List<TempContent<Content>> {temp};
            IDictionary<int, PropertyCollection> properties = GetPropertyCollections(ltemp);
            content.Properties = properties[dto.ContentVersionDto.Id];

            // set variations, if varying
            if (contentType?.VariesByCulture() ?? false)
            {
                IDictionary<int, List<ContentVariation>> contentVariations = GetContentVariations(ltemp);
                IDictionary<int, List<EntityVariation>> elementVariations = GetEntityVariations(ltemp);
                SetVariations(content, contentVariations, elementVariations);
            }

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }
        finally
        {
            content.EnableChangeTracking();
        }
    }

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Element;

    // TODO ELEMENTS: if this cannot be removed, make the one in the base protected
    // gets the COALESCE expression for variant/invariant name
    private string VariantNameSqlExpression
        => SqlContext.VisitDto<ContentVersionCultureVariationDto, NodeDto>((ccv, node) => ccv.Name ?? node.Text, "ccv")
            .Sql;

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentSchedule + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.User2NodeNotify + " WHERE nodeId = @id",
            // TODO ELEMENTS: include these if applicable, or clean up if not
            // "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2GranularPermission + " WHERE uniqueId IN (SELECT uniqueId FROM umbracoNode WHERE id = @id)",
            // "DELETE FROM " + Constants.DatabaseSchema.Tables.UserStartNode + " WHERE startNode = @id",
            // "UPDATE " + Constants.DatabaseSchema.Tables.UserGroup +
            // " SET startContentId = NULL WHERE startContentId = @id",
            // "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE parentId = @id",
            // "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE childId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Element + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ElementCultureVariation + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ElementVersion + " WHERE id IN (SELECT id FROM " +
            Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE versionId IN (SELECT id FROM " +
            Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersionCultureVariation +
            " WHERE versionId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion +
            " WHERE nodeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Content Repository

    public int CountPublished(string? contentTypeAlias = null)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();
        if (contentTypeAlias.IsNullOrWhiteSpace())
        {
            sql.SelectCount()
                .From<NodeDto>()
                .InnerJoin<ElementDto>()
                .On<NodeDto, ElementDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<ElementDto>(x => x.Published);
        }
        else
        {
            sql.SelectCount()
                .From<NodeDto>()
                .InnerJoin<ContentDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ElementDto>()
                .On<NodeDto, ElementDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias)
                .Where<ElementDto>(x => x.Published);
        }

        return Database.ExecuteScalar<int>(sql);
    }

    /// <inheritdoc />
    public override IEnumerable<IElement> GetPage(IQuery<IElement>? query,
        long pageIndex, int pageSize, out long totalRecords,
        IQuery<IElement>? filter, Ordering? ordering)
    {
        Sql<ISqlContext>? filterSql = null;

        // if we have a filter, map its clauses to an Sql statement
        if (filter != null)
        {
            // if the clause works on "name", we need to swap the field and use the variantName instead,
            // so that querying also works on variant content (for instance when searching a listview).

            // figure out how the "name" field is going to look like - so we can look for it
            var nameField = SqlContext.VisitModelField<IElement>(x => x.Name);

            filterSql = Sql();
            foreach (Tuple<string, object[]> filterClause in filter.GetWhereClauses())
            {
                var clauseSql = filterClause.Item1;
                var clauseArgs = filterClause.Item2;

                // replace the name field
                // we cannot reference an aliased field in a WHERE clause, so have to repeat the expression here
                clauseSql = clauseSql.Replace(nameField, VariantNameSqlExpression);

                // append the clause
                filterSql.Append($"AND ({clauseSql})", clauseArgs);
            }
        }

        return GetPage<ElementDto>(query, pageIndex, pageSize, out totalRecords,
            x => MapDtosToContent(x),
            filterSql,
            ordering);
    }

    // NOTE: Elements cannot have unpublished parents
    public bool IsPathPublished(IElement? content)
        => content is { Trashed: false, Published: true };

    #endregion

    #region Recycle Bin

    public override int RecycleBinId => Constants.System.RecycleBinContent;

    public bool RecycleBinSmells()
    {
        IAppPolicyCache cache = _appCaches.RuntimeCache;
        var cacheKey = CacheKeys.ContentRecycleBinCacheKey;

        // always cache either true or false
        return cache.GetCacheItem(cacheKey, () => CountChildren(RecycleBinId) > 0);
    }

    #endregion

    #region Schedule

    /// <inheritdoc />
    public void ClearSchedule(DateTime date)
    {
        Sql<ISqlContext> sql = Sql().Delete<ContentScheduleDto>().Where<ContentScheduleDto>(x => x.Date <= date);
        Database.Execute(sql);
    }

    /// <inheritdoc />
    public void ClearSchedule(DateTime date, ContentScheduleAction action)
    {
        var a = action.ToString();
        Sql<ISqlContext> sql = Sql().Delete<ContentScheduleDto>()
            .Where<ContentScheduleDto>(x => x.Date <= date && x.Action == a);
        Database.Execute(sql);
    }

    private Sql GetSqlForHasScheduling(ContentScheduleAction action, DateTime date)
    {
        SqlTemplate template = SqlContext.Templates.Get("Umbraco.Core.ElementRepository.GetSqlForHasScheduling",
            tsql => tsql
                .SelectCount()
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x =>
                    x.Action == SqlTemplate.Arg<string>("action") && x.Date <= SqlTemplate.Arg<DateTime>("date")));

        Sql<ISqlContext> sql = template.Sql(action.ToString(), date);
        return sql;
    }

    public bool HasContentForExpiration(DateTime date)
    {
        Sql sql = GetSqlForHasScheduling(ContentScheduleAction.Expire, date);
        return Database.ExecuteScalar<int>(sql) > 0;
    }

    public bool HasContentForRelease(DateTime date)
    {
        Sql sql = GetSqlForHasScheduling(ContentScheduleAction.Release, date);
        return Database.ExecuteScalar<int>(sql) > 0;
    }

    /// <inheritdoc />
    public IEnumerable<IElement> GetContentForRelease(DateTime date)
    {
        var action = ContentScheduleAction.Release.ToString();

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many)
            .WhereIn<NodeDto>(x => x.NodeId, Sql()
                .Select<ContentScheduleDto>(x => x.NodeId)
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Action == action && x.Date <= date));

        AddGetByQueryOrderBy(sql);

        return MapDtosToContent(Database.Fetch<ElementDto>(sql));
    }

    /// <inheritdoc />
    public IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(int[] contentIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<ContentScheduleDto>()
            .From<ContentScheduleDto>()
            .WhereIn<ContentScheduleDto>(contentScheduleDto => contentScheduleDto.NodeId, contentIds);

        List<ContentScheduleDto>? contentScheduleDtos = Database.Fetch<ContentScheduleDto>(sql);

        IDictionary<int, IEnumerable<ContentSchedule>> dictionary = contentScheduleDtos
            .GroupBy(contentSchedule => contentSchedule.NodeId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(scheduleDto => new ContentSchedule(
                        scheduleDto.Id,
                        LanguageRepository.GetIsoCodeById(scheduleDto.LanguageId) ?? Constants.System.InvariantCulture,
                        scheduleDto.Date,
                        scheduleDto.Action == ContentScheduleAction.Release.ToString()
                            ? ContentScheduleAction.Release
                            : ContentScheduleAction.Expire))
                    .ToList().AsEnumerable()); // We have to materialize it here,
                                               // to avoid this being used after the scope is disposed.

        return dictionary;
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetScheduledContentKeys(Guid[] keys)
    {
        var action = ContentScheduleAction.Release.ToString();
        DateTime now = DateTime.UtcNow;

        Sql<ISqlContext> sql = SqlContext.Sql();
        sql
            .Select<NodeDto>(x => x.UniqueId)
            .From<ElementDto>()
            .InnerJoin<ContentDto>().On<DocumentDto, ElementDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .WhereIn<NodeDto>(x => x.UniqueId, keys)
            .WhereIn<NodeDto>(x => x.NodeId, Sql()
                .Select<ContentScheduleDto>(x => x.NodeId)
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Action == action && x.Date >= now));

        return Database.Fetch<Guid>(sql);
    }

    /// <inheritdoc />
    public IEnumerable<IElement> GetContentForExpiration(DateTime date)
    {
        var action = ContentScheduleAction.Expire.ToString();

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many)
            .WhereIn<NodeDto>(x => x.NodeId, Sql()
                .Select<ContentScheduleDto>(x => x.NodeId)
                .From<ContentScheduleDto>()
                .Where<ContentScheduleDto>(x => x.Action == action && x.Date <= date));

        AddGetByQueryOrderBy(sql);

        return MapDtosToContent(Database.Fetch<ElementDto>(sql));
    }

    #endregion
}
