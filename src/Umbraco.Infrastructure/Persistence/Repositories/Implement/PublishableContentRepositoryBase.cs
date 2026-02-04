using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IPublishableContentBase" />.
/// </summary>
internal abstract class PublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto>
    : ContentRepositoryBase<int, TEntity, TRepository>, IReadRepository<Guid, TEntity>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto
    where TContentCultureVariationDto : class, ICultureVariationDto, new()
{
    private readonly IJsonSerializer _serializer;
    private readonly ITagRepository _tagRepository;
    private readonly EntityByGuidReadRepository _entityByGuidReadRepository;
    private readonly AppCaches _appCaches;

    protected abstract string RecycleBinCacheKey { get; }

    protected abstract TEntity MapDtoToContent(TEntityDto dto);

    protected abstract IEnumerable<TEntity> MapDtosToContent(
        List<TEntityDto> dtos,
        bool withCache = false,
        string[]? propertyAliases = null,
        bool loadTemplates = true,
        bool loadVariants = true);

    protected PublishableContentRepositoryBase(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<ContentRepositoryBase<int, TEntity, TRepository>> logger,
        ILoggerFactory loggerFactory,
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
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _appCaches = appCaches;
        _tagRepository = tagRepository;
        _serializer = serializer;
        _entityByGuidReadRepository = new EntityByGuidReadRepository(
            this,
            scopeAccessor,
            appCaches,
            loggerFactory.CreateLogger<EntityByGuidReadRepository>(),
            repositoryCacheVersionService,
            cacheSyncService);
    }

    /// <summary>
    ///     Default is to always ensure all entities have unique names
    /// </summary>
    protected virtual bool EnsureUniqueNaming { get; } = true;

    /// <inheritdoc />
    public ContentScheduleCollection GetContentSchedule(int contentId)
    {
        var result = new ContentScheduleCollection();

        List<ContentScheduleDto>? scheduleDtos = Database.Fetch<ContentScheduleDto>(Sql()
            .Select<ContentScheduleDto>()
            .From<ContentScheduleDto>()
            .Where<ContentScheduleDto>(x => x.NodeId == contentId));

        foreach (ContentScheduleDto? scheduleDto in scheduleDtos)
        {
            result.Add(new ContentSchedule(
                scheduleDto.Id,
                LanguageRepository.GetIsoCodeById(scheduleDto.LanguageId) ?? Constants.System.InvariantCulture,
                scheduleDto.Date,
                scheduleDto.Action == ContentScheduleAction.Release.ToString()
                    ? ContentScheduleAction.Release
                    : ContentScheduleAction.Expire));
        }

        return result;
    }

    protected override string ApplySystemOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
    {
        // note: 'updater' is the user who created the latest draft version,
        //       we don't have an 'updater' per culture (should we?)
        if (ordering.OrderBy.InvariantEquals("updater"))
        {
            Sql<ISqlContext> joins = Sql()
                .InnerJoin<UserDto>("updaterUser")
                .On<ContentVersionDto, UserDto>(
                (version, user) => version.UserId == user.Id,
                    aliasRight: "updaterUser");

            // see notes in ApplyOrdering: the field MUST be selected + aliased
            sql = Sql(
                InsertBefore(
                    sql,
                    "FROM",
                    ", " + SqlSyntax.GetFieldName<UserDto>(x => x.UserName, "updaterUser") + " AS ordering "),
                sql.Arguments);

            sql = InsertJoins(sql, joins);

            return "ordering";
        }

        if (ordering.OrderBy.InvariantEquals("published"))
        {
            // no culture, assume invariant and simply order by published.
            if (ordering.Culture.IsNullOrWhiteSpace())
            {
                return SqlSyntax.GetFieldName<TEntityDto>(x => x.Published);
            }

            // invariant: left join will yield NULL and we must use pcv to determine published
            // variant: left join may yield NULL or something, and that determines published

            Sql<ISqlContext> joins = Sql()
                .InnerJoin<ContentTypeDto>("ctype").On<ContentDto, ContentTypeDto>(
                    (content, contentType) => content.ContentTypeId == contentType.NodeId, aliasRight: "ctype")
                // left join on optional culture variation
                //the magic "[[[ISOCODE]]]" parameter value will be replaced in ContentRepositoryBase.GetPage() by the actual ISO code
                .LeftJoin<ContentVersionCultureVariationDto>(
                    nested => nested.InnerJoin<LanguageDto>("langp")
                    .On<ContentVersionCultureVariationDto, LanguageDto>(
                            (ccv, lang) => ccv.LanguageId == lang.Id && lang.IsoCode == "[[[ISOCODE]]]",
                            "ccvp",
                            "langp"),
                    "ccvp")
                .On<ContentVersionDto, ContentVersionCultureVariationDto>(
                (version, ccv) => version.Id == ccv.VersionId,
                    "pcv",
                    "ccvp");

            sql = InsertJoins(sql, joins);

            // see notes in ApplyOrdering: the field MUST be selected + aliased, and we cannot have
            // the whole CASE fragment in ORDER BY due to it not being detected by NPoco
            var sqlText = InsertBefore(
                sql.SQL,
                "FROM",

                // when invariant, ie 'variations' does not have the culture flag (value 1), it should be safe to simply use the published flag on umbracoDocument/umbracoElement,
                // otherwise check if there's a version culture variation for the lang, via ccv.id
                $", (CASE WHEN (ctype.variations & 1) = 0 THEN ({SqlSyntax.GetFieldName<TEntityDto>(x => x.Published)}) ELSE (CASE WHEN ccvp.id IS NULL THEN 0 ELSE 1 END) END) AS ordering "); // trailing space is important!

            sql = Sql(sqlText, sql.Arguments);

            return "ordering";
        }

        return base.ApplySystemOrdering(ref sql, ordering);
    }

    protected void SetVariations(
        TEntity? content,
        IDictionary<int, List<ContentVariation>> contentVariations,
        IDictionary<int, List<EntityVariation>> entityVariations)
    {
        if (content is null)
        {
            return;
        }

        if (contentVariations.TryGetValue(content.VersionId, out List<ContentVariation>? contentVariation))
        {
            foreach (ContentVariation v in contentVariation)
            {
                content.SetCultureInfo(v.Culture, v.Name, v.Date.EnsureUtc());
            }
        }

        if (content.PublishedState is PublishedState.Published && content.PublishedVersionId > 0 && contentVariations.TryGetValue(content.PublishedVersionId, out contentVariation))
        {
            foreach (ContentVariation v in contentVariation)
            {
                content.SetPublishInfo(v.Culture, v.Name, v.Date.EnsureUtc());
            }
        }

        if (entityVariations.TryGetValue(content.Id, out List<EntityVariation>? entityVariation))
        {
            content.SetCultureEdited(entityVariation.Where(x => x.Edited).Select(x => x.Culture));
        }
    }

    protected IDictionary<int, List<ContentVariation>> GetContentVariations<T>(List<TempContent<T>> temps)
        where T : class, IContentBase
    {
        var versions = new List<int>();
        foreach (TempContent<T> temp in temps)
        {
            versions.Add(temp.VersionId);
            if (temp.PublishedVersionId > 0)
            {
                versions.Add(temp.PublishedVersionId);
            }
        }

        if (versions.Count == 0)
        {
            return new Dictionary<int, List<ContentVariation>>();
        }

        IEnumerable<ContentVersionCultureVariationDto> dtos =
            Database.FetchByGroups<ContentVersionCultureVariationDto, int>(
                versions,
                Constants.Sql.MaxParameterCount,
                batch
                    => Sql()
                        .Select<ContentVersionCultureVariationDto>()
                        .From<ContentVersionCultureVariationDto>()
                        .WhereIn<ContentVersionCultureVariationDto>(x => x.VersionId, batch));

        var variations = new Dictionary<int, List<ContentVariation>>();

        foreach (ContentVersionCultureVariationDto dto in dtos)
        {
            if (!variations.TryGetValue(dto.VersionId, out List<ContentVariation>? variation))
            {
                variations[dto.VersionId] = variation = new List<ContentVariation>();
            }

            variation.Add(new ContentVariation
            {
                Culture = LanguageRepository.GetIsoCodeById(dto.LanguageId),
                Name = dto.Name,
                Date = dto.UpdateDate
            });
        }

        return variations;
    }

    protected IDictionary<int, List<EntityVariation>> GetEntityVariations<T>(List<TempContent<T>> temps)
        where T : class, IContentBase
    {
        IEnumerable<int> ids = temps.Select(x => x.Id);

        IEnumerable<TContentCultureVariationDto> dtos = Database.FetchByGroups<TContentCultureVariationDto, int>(
            ids,
            Constants.Sql.MaxParameterCount,
            batch => Sql()
                        .Select<TContentCultureVariationDto>()
                        .From<TContentCultureVariationDto>()
                        .WhereIn<TContentCultureVariationDto>(x => x.NodeId, batch));

        var variations = new Dictionary<int, List<EntityVariation>>();

        foreach (TContentCultureVariationDto dto in dtos)
        {
            if (!variations.TryGetValue(dto.NodeId, out List<EntityVariation>? variation))
            {
                variations[dto.NodeId] = variation = new List<EntityVariation>();
            }

            variation.Add(new EntityVariation
            {
                Culture = LanguageRepository.GetIsoCodeById(dto.LanguageId),
                Edited = dto.Edited
            });
        }

        return variations;
    }

    private IEnumerable<ContentVersionCultureVariationDto> GetContentVariationDtos(TEntity content, bool publishing)
    {
        if (content.CultureInfos is not null)
        {
            // create dtos for the 'current' (non-published) version, all cultures
            // ReSharper disable once UseDeconstruction
            foreach (ContentCultureInfos cultureInfo in content.CultureInfos)
            {
                yield return new ContentVersionCultureVariationDto
                {
                    VersionId = content.VersionId,
                    LanguageId =
                        LanguageRepository.GetIdByIsoCode(cultureInfo.Culture) ??
                        throw new InvalidOperationException("Not a valid culture."),
                    Culture = cultureInfo.Culture,
                    Name = cultureInfo.Name,
                    UpdateDate =
                        content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue // we *know* there is a value
                };
            }
        }

        // if not publishing, we're just updating the 'current' (non-published) version,
        // so there are no DTOs to create for the 'published' version which remains unchanged
        if (!publishing)
        {
            yield break;
        }

        if (content.PublishCultureInfos is not null)
        {
            // create dtos for the 'published' version, for published cultures (those having a name)
            // ReSharper disable once UseDeconstruction
            foreach (ContentCultureInfos cultureInfo in content.PublishCultureInfos)
            {
                yield return new ContentVersionCultureVariationDto
                {
                    VersionId = content.PublishedVersionId,
                    LanguageId =
                        LanguageRepository.GetIdByIsoCode(cultureInfo.Culture) ??
                        throw new InvalidOperationException("Not a valid culture."),
                    Culture = cultureInfo.Culture,
                    Name = cultureInfo.Name,
                    UpdateDate =
                        content.GetPublishDate(cultureInfo.Culture) ?? DateTime.MinValue // we *know* there is a value
                };
            }
        }
    }

    private IEnumerable<TContentCultureVariationDto> GetEntityVariationDtos(
        TEntity content,
        HashSet<string> editedCultures)
    {
        IEnumerable<string>
            allCultures = content.AvailableCultures.Union(content.PublishedCultures); // union = distinct
        foreach (var culture in allCultures)
        {
            var dto = new TContentCultureVariationDto
            {
                NodeId = content.Id,
                LanguageId =
                    LanguageRepository.GetIdByIsoCode(culture) ??
                    throw new InvalidOperationException("Not a valid culture."),
                Culture = culture,
                Name = content.GetCultureName(culture) ?? content.GetPublishName(culture),
                Available = content.IsCultureAvailable(culture),
                Published = content.IsCulturePublished(culture),
                // note: can't use IsCultureEdited at that point - hasn't been updated yet - see PersistUpdatedItem
                Edited = content.IsCultureAvailable(culture) &&
                         (!content.IsCulturePublished(culture) ||
                          (editedCultures != null && editedCultures.Contains(culture)))
            };

            yield return dto;
        }
    }

    protected sealed class ContentVariation
    {
        public string? Culture { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
    }

    protected sealed class EntityVariation
    {
        public string? Culture { get; set; }
        public bool Edited { get; set; }
    }

    #region Repository Base

    /// <inheritdoc />
    public override void Save(TEntity entity)
    {
        base.Save(entity);

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
        _entityByGuidReadRepository.PopulateCacheByKey(entity);
    }

    protected override TEntity? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<NodeDto>(x => x.NodeId == id);

        TEntityDto? dto = Database.FirstOrDefault<TEntityDto>(sql);
        if (dto is null)
        {
            return null;
        }

        TEntity content = MapDtoToContent(dto);

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
        _entityByGuidReadRepository.PopulateCacheByKey(content);

        return content;
    }

    protected override IEnumerable<TEntity> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, ids);
        }

        // MapDtosToContent returns a materialized array, so this is safe to enumerate multiple times.
        IEnumerable<TEntity> contents = MapDtosToContent(Database.Fetch<TEntityDto>(sql));

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database.
        _entityByGuidReadRepository.PopulateCacheByKey(contents);

        return contents;
    }

    protected override IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(QueryType.Many);

        var translator = new SqlTranslator<TEntity>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        AddGetByQueryOrderBy(sql);

        return MapDtosToContent(Database.Fetch<TEntityDto>(sql));
    }

    protected void AddGetByQueryOrderBy(Sql<ISqlContext> sql) =>
        sql
            .OrderBy<NodeDto>(x => x.Level)
            .OrderBy<NodeDto>(x => x.SortOrder);

    protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType) => GetBaseQuery(queryType, true);

    private string VariantNameSqlExpression
        => SqlContext.VisitDto<ContentVersionCultureVariationDto, NodeDto>((ccv, node) => ccv.Name ?? node.Text, "ccv")
            .Sql;

    protected Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        switch (queryType)
        {
            case QueryType.Count:
                sql = sql.SelectCount();
                break;
            case QueryType.Ids:
                sql = sql.Select<TEntityDto>(x => x.NodeId);
                break;
            case QueryType.Single:
            case QueryType.Many:
                // R# may flag this ambiguous and red-squiggle it, but it is not
                sql = sql.Select<TEntityDto>(r =>
                        r.Select(entityDto => entityDto.ContentDto, r1 =>
                                r1.Select(contentDto => contentDto.NodeDto))
                            .Select(entityDto => entityDto.ContentVersionDto, r1 =>
                                r1.Select(entityVersionDto => entityVersionDto.ContentVersionDto))
                            .Select(entityDto => entityDto.PublishedVersionDto, "pdv", r1 =>
                                r1.Select(entityVersionDto => entityVersionDto!.ContentVersionDto, "pcv")))

                    // select the variant name, coalesce to the invariant name, as "variantName"
                    .AndSelect(VariantNameSqlExpression + " AS variantName");
                break;
        }

        sql
            .From<TEntityDto>()
            .InnerJoin<ContentDto>().On<TEntityDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)

            // inner join on mandatory edited version
            .InnerJoin<ContentVersionDto>()
            .On<TEntityDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<TContentVersionDto>()
            .On<ContentVersionDto, TContentVersionDto>((left, right) => left.Id == right.Id)

            // left join on optional published version
            .LeftJoin<ContentVersionDto>(
                nested => nested.InnerJoin<TContentVersionDto>("pdv")
                    .On<ContentVersionDto, TContentVersionDto>(
                        (left, right) => left.Id == right.Id && right.Published,
                            "pcv",
                            "pdv"),
                        "pcv")
            .On<TEntityDto, ContentVersionDto>(
                (left, right) => left.NodeId == right.NodeId,
                aliasRight: "pcv")

            // TODO: should we be joining this when the query type is not single/many?
            // left join on optional culture variation
            //the magic "[[[ISOCODE]]]" parameter value will be replaced in ContentRepositoryBase.GetPage() by the actual ISO code
            .LeftJoin<ContentVersionCultureVariationDto>(
                nested => nested.InnerJoin<LanguageDto>("lang")
                .On<ContentVersionCultureVariationDto, LanguageDto>(
                        (ccv, lang) => ccv.LanguageId == lang.Id && lang.IsoCode == "[[[ISOCODE]]]",
                        "ccv",
                        "lang"),
                "ccv")
                .On<ContentVersionDto, ContentVersionCultureVariationDto>(
                        (version, ccv) => version.Id == ccv.VersionId,
                    aliasRight: "ccv");

        sql
            .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        // this would ensure we don't get the published version - keep for reference
        //sql
        //    .WhereAny(
        //        x => x.Where<ContentVersionDto, ContentVersionDto>((x1, x2) => x1.Id != x2.Id, alias2: "pcv"),
        //        x => x.WhereNull<ContentVersionDto>(x1 => x1.Id, "pcv")
        //    );

        if (current)
        {
            sql.Where<ContentVersionDto>(x => x.Current); // always get the current version
        }

        return sql;
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);

    // ah maybe not, that what's used for eg Exists in base repo
    protected override string GetBaseWhereClause() => $"{QuoteTableName(NodeDto.TableName)}.id = @id";

    #endregion

    #region Versions


    public override IEnumerable<TEntity> GetAllVersions(int nodeId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many, false)
            .Where<NodeDto>(x => x.NodeId == nodeId)
            .OrderByDescending<ContentVersionDto>(x => x.Current)
            .AndByDescending<ContentVersionDto>(x => x.VersionDate);

        return MapDtosToContent(Database.Fetch<TEntityDto>(sql), true);
    }

    // TODO: This method needs to return a readonly version of IContent! The content returned
    // from this method does not contain all of the data required to re-persist it and if that
    // is attempted some odd things will occur.
    // Either we create an IContentReadOnly (which ultimately we should for vNext so we can
    // differentiate between methods that return entities that can be re-persisted or not), or
    // in the meantime to not break API compatibility, we can add a property to IContentBase
    // (or go further and have it on IUmbracoEntity): "IsReadOnly" and if that is true we throw
    // an exception if that entity is passed to a Save method.
    // Ideally we return "Slim" versions of content for all sorts of methods here and in ContentService.
    // Perhaps another non-breaking alternative is to have new services like IContentServiceReadOnly
    // which can return IContentReadOnly.
    // We have the ability with `MapDtosToContent` to reduce the amount of data looked up for a
    // content item. Ideally for paged data that populates list views, these would be ultra slim
    // content items, there's no reason to populate those with really anything apart from property data,
    // but until we do something like the above, we can't do that since it would be breaking and unclear.
    public override IEnumerable<TEntity> GetAllVersionsSlim(int nodeId, int skip, int take)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many, false)
            .Where<NodeDto>(x => x.NodeId == nodeId)
            .OrderByDescending<ContentVersionDto>(x => x.Current)
            .AndByDescending<ContentVersionDto>(x => x.VersionDate);

        var pageIndex = skip / take;

        return MapDtosToContent(
            Database.Page<TEntityDto>(pageIndex + 1, take, sql).Items,
            true,
            // load bare minimum, need variants though since this is used to rollback with variants
            propertyAliases: [],
            loadTemplates: false,
            loadVariants: true);
    }

    public override TEntity? GetVersion(int versionId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single, false)
            .Where<ContentVersionDto>(x => x.Id == versionId);

        TEntityDto? dto = Database.Fetch<TEntityDto>(sql).FirstOrDefault();
        return dto == null ? null : MapDtoToContent(dto);
    }

    // deletes a specific version
    public override void DeleteVersion(int versionId)
    {
        // TODO: test object node type?

        // get the version we want to delete
        SqlTemplate template = SqlContext.Templates.Get($"Umbraco.Core.{GetType().Name}.GetVersion", tsql =>
            tsql.Select<ContentVersionDto>()
                .AndSelect<TContentVersionDto>()
                .From<ContentVersionDto>()
                .InnerJoin<TContentVersionDto>()
                .On<ContentVersionDto, TContentVersionDto>((c, d) => c.Id == d.Id)
                .Where<TContentVersionDto>(x => x.Id == SqlTemplate.Arg<int>("versionId")));
        TContentVersionDto? versionDto =
            Database.Fetch<TContentVersionDto>(template.Sql(new { versionId })).FirstOrDefault();

        // nothing to delete
        if (versionDto == null)
        {
            return;
        }

        // don't delete the current or published version
        if (versionDto.ContentVersionDto.Current)
        {
            throw new InvalidOperationException("Cannot delete the current version.");
        }

        if (versionDto.Published)
        {
            throw new InvalidOperationException("Cannot delete the published version.");
        }

        PerformDeleteVersion(versionDto.ContentVersionDto.NodeId, versionId);
    }

    //  deletes all versions of an entity, older than a date.
    public override void DeleteVersions(int nodeId, DateTime versionDate)
    {
        // TODO: test object node type?

        // get the versions we want to delete, excluding the current one
        SqlTemplate template = SqlContext.Templates.Get($"Umbraco.Core.{GetType().Name}.GetVersions", tsql =>
            tsql.Select<ContentVersionDto>()
                .From<ContentVersionDto>()
                .InnerJoin<TContentVersionDto>()
                .On<ContentVersionDto, TContentVersionDto>((c, d) => c.Id == d.Id)
                .Where<ContentVersionDto>(x =>
                    x.NodeId == SqlTemplate.Arg<int>("nodeId") && !x.Current &&
                    x.VersionDate < SqlTemplate.Arg<DateTime>("versionDate"))
                .Where<TContentVersionDto>(x => !x.Published));
        List<ContentVersionDto>? versionDtos =
            Database.Fetch<ContentVersionDto>(template.Sql(new { nodeId, versionDate }));
        foreach (ContentVersionDto? versionDto in versionDtos)
        {
            PerformDeleteVersion(versionDto.NodeId, versionDto.Id);
        }
    }

    protected override void PerformDeleteVersion(int id, int versionId)
    {
        Sql<ISqlContext> sql = Sql().Delete<PropertyDataDto>(x => x.VersionId == versionId);
        Database.Execute(sql);

        sql = Sql().Delete<ContentVersionCultureVariationDto>(x => x.VersionId == versionId);
        Database.Execute(sql);

        sql = Sql().Delete<TContentVersionDto>(x => x.Id == versionId);
        Database.Execute(sql);

        sql = Sql().Delete<ContentVersionDto>(x => x.Id == versionId);
        Database.Execute(sql);
    }

    #endregion

    #region Persist

    protected override void PersistNewItem(TEntity entity)
    {
        entity.AddingEntity();

        var publishing = entity.PublishedState == PublishedState.Publishing;

        // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
        if (entity is IContent content)
        {
            // ensure that the default template is assigned
            if (content.TemplateId.HasValue == false)
            {
                content.TemplateId = entity.ContentType.DefaultTemplate?.Id;
            }
        }

        // sanitize names
        SanitizeNames(entity, publishing);

        // ensure that strings don't contain characters that are invalid in xml
        // TODO: do we really want to keep doing this here?
        entity.SanitizeEntityPropertiesForXmlStorage();

        // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
        // create the dto
        TEntityDto dto = (entity is IContent tempContent
            ? ContentBaseFactory.BuildDto(tempContent, NodeObjectTypeId) as TEntityDto
            : entity is IElement tempElement
                ? ContentBaseFactory.BuildDto(tempElement, NodeObjectTypeId) as TEntityDto
                : throw new InvalidOperationException("Unsupported entity type"))!;

        // derive path and level from parent
        NodeDto parent = GetParentNodeDto(entity.ParentId);
        var level = parent.Level + 1;

        var calculateSortOrder = (entity is { HasIdentity: false, SortOrder: 0 } && entity.IsPropertyDirty(nameof(entity.SortOrder)) is false) // SortOrder was not updated from it's default value
                                 || SortorderExists(entity.ParentId, entity.SortOrder);
        // if the sortorder of the entity already exists get a new one, else use the sortOrder of the entity
        var sortOrder = calculateSortOrder ? GetNewChildSortOrder(entity.ParentId, 0) : entity.SortOrder;

        // persist the node dto
        NodeDto nodeDto = dto.ContentDto.NodeDto;
        nodeDto.Path = parent.Path;
        nodeDto.Level = Convert.ToInt16(level);
        nodeDto.SortOrder = sortOrder;

        // see if there's a reserved identifier for this unique id
        // and then either update or insert the node dto
        var id = GetReservedId(nodeDto.UniqueId);
        if (id > 0)
        {
            nodeDto.NodeId = id;
        }
        else
        {
            Database.Insert(nodeDto);
        }

        nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
        nodeDto.ValidatePathWithException();
        Database.Update(nodeDto);

        // update entity
        entity.Id = nodeDto.NodeId;
        entity.Path = nodeDto.Path;
        entity.SortOrder = sortOrder;
        entity.Level = level;

        // persist the content dto
        ContentDto contentDto = dto.ContentDto;
        contentDto.NodeId = nodeDto.NodeId;
        Database.Insert(contentDto);

        // persist the content version dto
        ContentVersionDto contentVersionDto = dto.ContentVersionDto.ContentVersionDto;
        contentVersionDto.NodeId = nodeDto.NodeId;
        contentVersionDto.Current = !publishing;
        Database.Insert(contentVersionDto);
        entity.VersionId = contentVersionDto.Id;

        // persist the entity version dto
        TContentVersionDto entityVersionDto = dto.ContentVersionDto;
        entityVersionDto.Id = entity.VersionId;
        if (publishing)
        {
            entityVersionDto.Published = true;
        }

        Database.Insert(entityVersionDto);

        // and again in case we're publishing immediately
        if (publishing)
        {
            entity.PublishedVersionId = entity.VersionId;
            contentVersionDto.Id = 0;
            contentVersionDto.Current = true;
            contentVersionDto.Text = entity.Name;
            Database.Insert(contentVersionDto);
            entity.VersionId = contentVersionDto.Id;

            entityVersionDto.Id = entity.VersionId;
            entityVersionDto.Published = false;
            Database.Insert(entityVersionDto);
        }

        // persist the property data
        IEnumerable<PropertyDataDto> propertyDataDtos = PropertyFactory.BuildDtos(
            entity.ContentType.Variations,
            entity.VersionId,
            entity.PublishedVersionId,
            entity.Properties,
            LanguageRepository,
            out var edited,
            out HashSet<string>? editedCultures);
        foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
        {
            Database.Insert(propertyDataDto);
        }

        // if !publishing, we may have a new name != current publish name,
        // also impacts 'edited'
        if (!publishing && entity.PublishName != entity.Name)
        {
            edited = true;
        }

        // persist the entity dto
        // at that point, when publishing, the entity still has its old Published value
        // so we need to explicitly update the dto to persist the correct value
        if (entity.PublishedState == PublishedState.Publishing)
        {
            dto.Published = true;
        }

        dto.NodeId = nodeDto.NodeId;
        entity.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited
        Database.Insert(dto);

        // persist the variations
        if (entity.ContentType.VariesByCulture())
        {
            // names also impact 'edited'
            // ReSharper disable once UseDeconstruction
            foreach (ContentCultureInfos cultureInfo in entity.CultureInfos!)
            {
                if (cultureInfo.Name != entity.GetPublishName(cultureInfo.Culture))
                {
                    (editedCultures ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(cultureInfo.Culture);
                }
            }

            // refresh content
            entity.SetCultureEdited(editedCultures!);

            // bump dates to align cultures to version
            entity.AdjustDates(contentVersionDto.VersionDate, publishing);

            // insert content variations
            Database.BulkInsertRecords(GetContentVariationDtos(entity, publishing));

            // insert entity variations
            Database.BulkInsertRecords(GetEntityVariationDtos(entity, editedCultures!));
        }

        // trigger here, before we reset Published etc
        // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
        // TODO: ELEMENTS: implement
        if (entity is IContent tempContent2)
        {
            OnUowRefreshedEntity(new ContentRefreshNotification(tempContent2, new EventMessages()));
        }

        // flip the entity's published property
        // this also flips its published state
        // note: what depends on variations (eg PublishNames) is managed directly by the content
        if (entity.PublishedState == PublishedState.Publishing)
        {
            entity.Published = true;
            // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
            if (entity is IContent tempContent3)
            {
                tempContent3.PublishTemplateId = tempContent3.TemplateId;
            }
            entity.PublisherId = entity.WriterId;
            entity.PublishName = entity.Name;
            entity.PublishDate = entity.UpdateDate;

            SetEntityTags(entity, _tagRepository, _serializer);
        }
        else if (entity.PublishedState == PublishedState.Unpublishing)
        {
            entity.Published = false;
            // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
            if (entity is IContent tempContent3)
            {
                tempContent3.PublishTemplateId = null;
            }
            entity.PublisherId = null;
            entity.PublishName = null;
            entity.PublishDate = null;

            ClearEntityTags(entity, _tagRepository);
        }

        entity.ResetDirtyProperties();

        // troubleshooting
        //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE published=1 AND nodeId=" + content.Id) > 1)
        //{
        //    Debugger.Break();
        //    throw new Exception("oops");
        //}
        //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE [current]=1 AND nodeId=" + content.Id) > 1)
        //{
        //    Debugger.Break();
        //    throw new Exception("oops");
        //}
    }

    protected override void PersistUpdatedItem(TEntity entity)
    {
        var isEntityDirty = entity.IsDirty();
        var editedSnapshot = entity.Edited;

        // check if we need to make any database changes at all
        if ((entity.PublishedState == PublishedState.Published || entity.PublishedState == PublishedState.Unpublished)
            && !isEntityDirty && !entity.IsAnyUserPropertyDirty())
        {
            return; // no change to save, do nothing, don't even update dates
        }

        // whatever we do, we must check that we are saving the current version
        ContentVersionDto? version = Database.Fetch<ContentVersionDto>(SqlContext.Sql().Select<ContentVersionDto>()
            .From<ContentVersionDto>().Where<ContentVersionDto>(x => x.Id == entity.VersionId)).FirstOrDefault();
        if (version == null || !version.Current)
        {
            throw new InvalidOperationException("Cannot save a non-current version.");
        }

        // update
        entity.UpdatingEntity();

        // Check if this entity is being moved as a descendant as part of a bulk moving operations.
        // In this case we can bypass a lot of the below operations which will make this whole operation go much faster.
        // When moving we don't need to create new versions, etc... because we cannot roll this operation back anyways.
        var isMoving = entity.IsMoving();
        // TODO: I'm sure we can also detect a "Copy" (of a descendant) operation and probably perform similar checks below.
        // There is probably more stuff that would be required for copying but I'm sure not all of this logic would be, we could more than likely boost
        // copy performance by 95% just like we did for Move


        var publishing = entity.PublishedState == PublishedState.Publishing;

        if (!isMoving)
        {
            // check if we need to create a new version
            if (publishing && entity.PublishedVersionId > 0)
            {
                // published version is not published anymore
                Database.Execute(Sql().Update<TContentVersionDto>(u => u.Set(x => x.Published, false))
                    .Where<TContentVersionDto>(x => x.Id == entity.PublishedVersionId));
            }

            // sanitize names
            SanitizeNames(entity, publishing);

            // ensure that strings don't contain characters that are invalid in xml
            // TODO: do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // if parent has changed, get path, level and sort order
            if (entity.IsPropertyDirty("ParentId"))
            {
                NodeDto parent = GetParentNodeDto(entity.ParentId);
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
            }
        }

        // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
        // create the dto
        TEntityDto dto = (entity is IContent tempContent
            ? ContentBaseFactory.BuildDto(tempContent, NodeObjectTypeId) as TEntityDto
            : entity is IElement tempElement
                ? ContentBaseFactory.BuildDto(tempElement, NodeObjectTypeId) as TEntityDto
                : throw new InvalidOperationException("Unsupported entity type"))!;

        // update the node dto
        NodeDto nodeDto = dto.ContentDto.NodeDto;
        nodeDto.ValidatePathWithException();
        Database.Update(nodeDto);

        if (!isMoving)
        {
            // update the content dto
            Database.Update(dto.ContentDto);

            // update the content & entity version dtos
            ContentVersionDto contentVersionDto = dto.ContentVersionDto.ContentVersionDto;
            TContentVersionDto entityVersionDto = dto.ContentVersionDto;
            if (publishing)
            {
                entityVersionDto.Published = true; // now published
                contentVersionDto.Current = false; // no more current
            }

            // Ensure existing version retains current preventCleanup flag (both saving and publishing).
            contentVersionDto.PreventCleanup = version.PreventCleanup;

            Database.Update(contentVersionDto);
            Database.Update(entityVersionDto);

            // and, if publishing, insert new content & entity version dtos
            if (publishing)
            {
                entity.PublishedVersionId = entity.VersionId;

                contentVersionDto.Id = 0; // want a new id
                contentVersionDto.Current = true; // current version
                contentVersionDto.Text = entity.Name;
                contentVersionDto.PreventCleanup = false; // new draft version disregards prevent cleanup flag
                Database.Insert(contentVersionDto);
                entity.VersionId = entityVersionDto.Id = contentVersionDto.Id; // get the new id

                entityVersionDto.Published = false; // non-published version
                Database.Insert(entityVersionDto);
            }

            // replace the property data (rather than updating)
            // only need to delete for the version that existed, the new version (if any) has no property data yet
            var versionToDelete = publishing ? entity.PublishedVersionId : entity.VersionId;

            // insert property data
            ReplacePropertyValues(
                entity,
                versionToDelete,
                publishing ? entity.PublishedVersionId : 0,
                out var edited,
                out HashSet<string>? editedCultures);

            // if !publishing, we may have a new name != current publish name,
            // also impacts 'edited'
            if (!publishing && entity.PublishName != entity.Name)
            {
                edited = true;
            }

            // To establish the new value of "edited" we compare all properties publishedValue to editedValue and look
            // for differences.
            //
            // If we SaveAndPublish but the publish fails (e.g. already scheduled for release)
            // we have lost the publishedValue on IContent (in memory vs database) so we cannot correctly make that comparison.
            //
            // This is a slight change to behaviour, historically a publish, followed by change & save, followed by undo change & save
            // would change edited back to false.
            if (!publishing && editedSnapshot)
            {
                edited = true;
            }

            if (entity.ContentType.VariesByCulture())
            {
                // names also impact 'edited'
                // ReSharper disable once UseDeconstruction
                foreach (ContentCultureInfos cultureInfo in entity.CultureInfos!)
                {
                    if (cultureInfo.Name != entity.GetPublishName(cultureInfo.Culture))
                    {
                        edited = true;
                        (editedCultures ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(cultureInfo
                            .Culture);

                        // TODO: change tracking
                        // at the moment, we don't do any dirty tracking on property values, so we don't know whether the
                        // culture has just been edited or not, so we don't update its update date - that date only changes
                        // when the name is set, and it all works because the controller does it - but, if someone uses a
                        // service to change a property value and save (without setting name), the update date does not change.
                    }
                }

                // refresh content
                entity.SetCultureEdited(editedCultures!);

                // bump dates to align cultures to version
                entity.AdjustDates(contentVersionDto.VersionDate, publishing);

                // replace the content version variations (rather than updating)
                // only need to delete for the version that existed, the new version (if any) has no property data yet
                Sql<ISqlContext> deleteContentVariations = Sql().Delete<ContentVersionCultureVariationDto>()
                    .Where<ContentVersionCultureVariationDto>(x => x.VersionId == versionToDelete);
                Database.Execute(deleteContentVariations);

                // replace the entity version variations (rather than updating)
                Sql<ISqlContext> deleteEntityVariations = Sql().Delete<TContentCultureVariationDto>()
                    .Where<TContentCultureVariationDto>(x => x.NodeId == entity.Id);
                Database.Execute(deleteEntityVariations);

                // TODO: NPoco InsertBulk issue?
                // we should use the native NPoco InsertBulk here but it causes problems (not sure exactly all scenarios)
                // but by using SQL Server and updating a variants name will cause: Unable to cast object of type
                // 'Umbraco.Core.Persistence.FaultHandling.RetryDbConnection' to type 'System.Data.SqlClient.SqlConnection'.
                // (same in PersistNewItem above)

                // insert content variations
                Database.BulkInsertRecords(GetContentVariationDtos(entity, publishing));

                // insert entity variations
                Database.BulkInsertRecords(GetEntityVariationDtos(entity, editedCultures!));
            }

            // update the entity dto
            // at that point, when un/publishing, the entity still has its old Published value
            // so we need to explicitly update the dto to persist the correct value
            if (entity.PublishedState == PublishedState.Publishing)
            {
                dto.Published = true;
            }
            else if (entity.PublishedState == PublishedState.Unpublishing)
            {
                dto.Published = false;
            }

            entity.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited
            Database.Update(dto);

            // if entity is publishing, update tags, else leave tags there
            // means that implicitly unpublished, or trashed, entities *still* have tags in db
            if (entity.PublishedState == PublishedState.Publishing)
            {
                SetEntityTags(entity, _tagRepository, _serializer);
            }
        }

        // trigger here, before we reset Published etc
        // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
        // TODO: ELEMENTS: implement
        if (entity is IContent tempContent2)
        {
            OnUowRefreshedEntity(new ContentRefreshNotification(tempContent2, new EventMessages()));
        }

        if (!isMoving)
        {
            // flip the entity's published property
            // this also flips its published state
            if (entity.PublishedState == PublishedState.Publishing)
            {
                entity.Published = true;
                // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
                if (entity is IContent tempContent3)
                {
                    tempContent3.PublishTemplateId = tempContent3.TemplateId;
                }
                entity.PublisherId = entity.WriterId;
                entity.PublishName = entity.Name;
                entity.PublishDate = entity.UpdateDate;

                SetEntityTags(entity, _tagRepository, _serializer);
            }
            else if (entity.PublishedState == PublishedState.Unpublishing)
            {
                entity.Published = false;
                // TODO: ELEMENTS: handle this by abstraction, not by hardcoding
                if (entity is IContent tempContent3)
                {
                    tempContent3.PublishTemplateId = null;
                }
                entity.PublisherId = null;
                entity.PublishName = null;
                entity.PublishDate = null;

                ClearEntityTags(entity, _tagRepository);
            }

            // TODO: note re. tags: explicitly unpublished entities have cleared tags, but masked or trashed entities *still* have tags in the db - so what?
        }

        entity.ResetDirtyProperties();

        // We need to flush the isolated cache by key explicitly here.
        // The ContentCacheRefresher does the same thing, but by the time it's invoked, custom notification handlers
        // might have already consumed the cached version (which at this point is the previous version).
        IsolatedCache.ClearByKey(RepositoryCacheKeys.GetKey<IContent, Guid>(entity.Key));

        // troubleshooting
        //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE published=1 AND nodeId=" + content.Id) > 1)
        //{
        //    Debugger.Break();
        //    throw new Exception("oops");
        //}
        //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE [current]=1 AND nodeId=" + content.Id) > 1)
        //{
        //    Debugger.Break();
        //    throw new Exception("oops");
        //}
    }

    /// <inheritdoc />
    public void PersistContentSchedule(IPublishableContentBase content, ContentScheduleCollection contentSchedule)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (contentSchedule == null)
        {
            throw new ArgumentNullException(nameof(contentSchedule));
        }

        var schedules = ContentBaseFactory.BuildScheduleDto(content, contentSchedule, LanguageRepository).ToList();

        //remove any that no longer exist
        IEnumerable<Guid> ids = schedules.Where(x => x.Model.Id != Guid.Empty).Select(x => x.Model.Id).Distinct();
        Database.Execute(Sql()
            .Delete<ContentScheduleDto>()
            .Where<ContentScheduleDto>(x => x.NodeId == content.Id)
            .WhereNotIn<ContentScheduleDto>(x => x.Id, ids));

        //add/update the rest
        foreach ((ContentSchedule model, ContentScheduleDto dto) in schedules)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = dto.Id = Guid.NewGuid();
                Database.Insert(dto);
            }
            else
            {
                Database.Update(dto);
            }
        }
    }

    protected override void PersistDeletedItem(TEntity entity)
    {
        // Raise event first else potential FK issues
        OnUowRemovingEntity(entity);

        //We need to clear out all access rules but we need to do this in a manual way since
        // nothing in that table is joined to a content id
        Sql<ISqlContext> subQuery = SqlContext.Sql()
            .Select<AccessRuleDto>(x => x.AccessId)
            .From<AccessRuleDto>()
            .InnerJoin<AccessDto>()
            .On<AccessRuleDto, AccessDto>(left => left.AccessId, right => right.Id)
            .Where<AccessDto>(dto => dto.NodeId == entity.Id);
        Database.Execute(SqlContext.SqlSyntax.GetDeleteSubquery("umbracoAccessRule", "accessId", subQuery));

        //now let the normal delete clauses take care of everything else
        base.PersistDeletedItem(entity);
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
                .InnerJoin<TEntityDto>()
                .On<NodeDto, DocumentDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<TEntityDto>(x => x.Published);
        }
        else
        {
            sql.SelectCount()
                .From<NodeDto>()
                .InnerJoin<ContentDto>()
                .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<TEntityDto>()
                .On<NodeDto, TEntityDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias)
                .Where<TEntityDto>(x => x.Published);
        }

        return Database.ExecuteScalar<int>(sql);
    }

    /// <inheritdoc />
    [Obsolete("Please use the method overload with all parameters. Scheduled for removal in Umbraco 19.")]
    public override IEnumerable<TEntity> GetPage(
        IQuery<TEntity>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<TEntity>? filter,
        Ordering? ordering)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases: null, filter: filter, ordering: ordering, loadTemplates: true);

    /// <inheritdoc />
    public override IEnumerable<TEntity> GetPage(
        IQuery<TEntity>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<TEntity>? filter,
        Ordering? ordering)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases, filter, ordering, loadTemplates: true);

    /// <inheritdoc />
    public IEnumerable<TEntity> GetPage(
        IQuery<TEntity>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<TEntity>? filter,
        Ordering? ordering,
        bool loadTemplates)
    {
        Sql<ISqlContext>? filterSql = null;

        // if we have a filter, map its clauses to an Sql statement
        if (filter != null)
        {
            // if the clause works on "name", we need to swap the field and use the variantName instead,
            // so that querying also works on variant content (for instance when searching a listview).

            // figure out how the "name" field is going to look like - so we can look for it
            var nameField = SqlContext.VisitModelField<TEntity>(x => x.Name);

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

        return GetPage<TEntityDto>(
            query,
            pageIndex,
            pageSize,
            out totalRecords,
            x => MapDtosToContent(x, propertyAliases: propertyAliases, loadTemplates: loadTemplates),
            filterSql,
            ordering);
    }

    #endregion

    #region Recycle Bin

    public bool RecycleBinSmells()
    {
        IAppPolicyCache cache = _appCaches.RuntimeCache;

        // always cache either true or false
        return cache.GetCacheItem(RecycleBinCacheKey, () => CountChildren(RecycleBinId) > 0);
    }

    #endregion

    #region Read Repository implementation for Guid keys

    public TEntity? Get(Guid id) => _entityByGuidReadRepository.Get(id);

    IEnumerable<TEntity> IReadRepository<Guid, TEntity>.GetMany(params Guid[]? ids) =>
        _entityByGuidReadRepository.GetMany(ids);

    public bool Exists(Guid id) => _entityByGuidReadRepository.Exists(id);

    /// <summary>
    /// Populates the int-keyed cache with the given entity.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(TEntity entity)
    {
        if (entity.HasIdentity)
        {
            var cacheKey = GetCacheKey(entity.Id);
            IsolatedCache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);
        }
    }

    /// <summary>
    /// Populates the int-keyed cache with the given entities.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(IEnumerable<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            PopulateCacheById(entity);
        }
    }

    private static string GetCacheKey(int id) => RepositoryCacheKeys.GetKey<TEntity>() + id;

    // reading repository purely for looking up by GUID
    // TODO: ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
    // This sub-repository pattern is super old and totally unecessary anymore, caching can be handled in much nicer ways without this
    private sealed class EntityByGuidReadRepository : EntityRepositoryBase<Guid, TEntity>
    {
        private readonly PublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto> _outerRepo;

        public EntityByGuidReadRepository(
            PublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto> outerRepo,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<EntityByGuidReadRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService) =>
            _outerRepo = outerRepo;

        protected override TEntity? PerformGet(Guid id)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.UniqueId == id);

            TEntityDto? dto = Database.FirstOrDefault<TEntityDto>(sql);

            if (dto == null)
            {
                return null;
            }

            TEntity content = _outerRepo.MapDtoToContent(dto);

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(content);

            return content;
        }

        protected override IEnumerable<TEntity> PerformGetAll(params Guid[]? ids)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Many);
            if (ids?.Length > 0)
            {
                sql.WhereIn<NodeDto>(x => x.UniqueId, ids);
            }

            // MapDtosToContent returns a materialized array, so this is safe to enumerate multiple times
            IEnumerable<TEntity> contents = _outerRepo.MapDtosToContent(Database.Fetch<TEntityDto>(sql));

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(contents);

            return contents;
        }

        protected override IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<string> GetDeleteClauses() =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistNewItem(TEntity entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistUpdatedItem(TEntity entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override string GetBaseWhereClause() =>
            throw new InvalidOperationException("This method won't be implemented.");

        /// <summary>
        /// Populates the GUID-keyed cache with the given entity.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        public void PopulateCacheByKey(TEntity entity)
        {
            if (entity.HasIdentity)
            {
                var cacheKey = GetCacheKey(entity.Key);
                IsolatedCache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);
            }
        }

        /// <summary>
        /// Populates the GUID-keyed cache with the given entities.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        public void PopulateCacheByKey(IEnumerable<TEntity> entities)
        {
            foreach (TEntity entity in entities)
            {
                PopulateCacheByKey(entity);
            }
        }

        private static string GetCacheKey(Guid key) => RepositoryCacheKeys.GetKey<TEntity>() + key;
    }

    #endregion

    #region Utilities

    private void SanitizeNames(TEntity content, bool publishing)
    {
        // a content item *must* have an invariant name, and invariant published name
        // else we just cannot write the invariant rows (node, content version...) to the database

        // ensure that we have an invariant name
        // invariant content = must be there already, else throw
        // variant content = update with default culture or anything really
        EnsureInvariantNameExists(content);

        // ensure that invariant name is unique
        EnsureInvariantNameIsUnique(content);

        // and finally,
        // ensure that each culture has a unique node name
        // no published name = not published
        // else, it needs to be unique
        EnsureVariantNamesAreUnique(content, publishing);
    }

    private void EnsureInvariantNameExists(TEntity content)
    {
        if (content.ContentType.VariesByCulture())
        {
            // content varies by culture
            // then it must have at least a variant name, else it makes no sense
            if (content.CultureInfos?.Count == 0)
            {
                throw new InvalidOperationException("Cannot save content with an empty name.");
            }

            // and then, we need to set the invariant name implicitly,
            // using the default culture if it has a name, otherwise anything we can
            var defaultCulture = LanguageRepository.GetDefaultIsoCode();
            content.Name = defaultCulture != null &&
                           (content.CultureInfos?.TryGetValue(defaultCulture, out ContentCultureInfos cultureName) ??
                            false)
                ? cultureName.Name!
                : content.CultureInfos![0].Name!;
        }
        else
        {
            // content is invariant, and invariant content must have an explicit invariant name
            if (string.IsNullOrWhiteSpace(content.Name))
            {
                throw new InvalidOperationException("Cannot save content with an empty name.");
            }
        }
    }

    private void EnsureInvariantNameIsUnique(TEntity content) =>
        content.Name = EnsureUniqueNodeName(content.ParentId, content.Name, content.Id);

    protected override string? EnsureUniqueNodeName(int parentId, string? nodeName, int id = 0) =>
        EnsureUniqueNaming == false ? nodeName : base.EnsureUniqueNodeName(parentId, nodeName, id);

    private SqlTemplate SqlEnsureVariantNamesAreUnique => SqlContext.Templates.Get(
        "Umbraco.Core.DomainRepository.EnsureVariantNamesAreUnique", tsql => tsql
            .Select<ContentVersionCultureVariationDto>(x => x.Id, x => x.Name, x => x.LanguageId)
            .From<ContentVersionCultureVariationDto>()
            .InnerJoin<ContentVersionDto>()
            .On<ContentVersionDto, ContentVersionCultureVariationDto>(x => x.Id, x => x.VersionId)
            .InnerJoin<NodeDto>().On<NodeDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
            .Where<ContentVersionDto>(x => x.Current == SqlTemplate.Arg<bool>("current"))
            .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") &&
                                 x.ParentId == SqlTemplate.Arg<int>("parentId") &&
                                 x.NodeId != SqlTemplate.Arg<int>("id"))
            .OrderBy<ContentVersionCultureVariationDto>(x => x.LanguageId));

    private void EnsureVariantNamesAreUnique(TEntity content, bool publishing)
    {
        if (!EnsureUniqueNaming || !content.ContentType.VariesByCulture() || content.CultureInfos?.Count == 0)
        {
            return;
        }

        // get names per culture, at same level (ie all siblings)
        Sql<ISqlContext> sql = SqlEnsureVariantNamesAreUnique.Sql(true, NodeObjectTypeId, content.ParentId, content.Id);
        var names = Database.Fetch<CultureNodeName>(sql)
            .GroupBy(x => x.LanguageId)
            .ToDictionary(x => x.Key, x => x);

        if (names.Count == 0)
        {
            return;
        }

        // note: the code below means we are going to unique-ify every culture names, regardless
        // of whether the name has changed (ie the culture has been updated) - some saving culture
        // fr-FR could cause culture en-UK name to change - not sure that is clean

        if (content.CultureInfos is null)
        {
            return;
        }

        foreach (ContentCultureInfos cultureInfo in content.CultureInfos)
        {
            var langId = LanguageRepository.GetIdByIsoCode(cultureInfo.Culture);
            if (!langId.HasValue)
            {
                continue;
            }

            if (!names.TryGetValue(langId.Value, out IGrouping<int, CultureNodeName>? cultureNames))
            {
                continue;
            }

            // get a unique name
            IEnumerable<SimilarNodeName> otherNames =
                cultureNames.Select(x => new SimilarNodeName { Id = x.Id, Name = x.Name });
            var uniqueName = SimilarNodeName.GetUniqueName(otherNames, 0, cultureInfo.Name);

            if (uniqueName == content.GetCultureName(cultureInfo.Culture))
            {
                continue;
            }

            // update the name, and the publish name if published
            content.SetCultureName(uniqueName, cultureInfo.Culture);
            if (publishing && (content.PublishCultureInfos?.ContainsKey(cultureInfo.Culture) ?? false))
            {
                content.SetPublishInfo(
                    cultureInfo.Culture,
                    uniqueName,
                    DateTime.UtcNow); //TODO: This is weird, this call will have already been made in the SetCultureName
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class CultureNodeName
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int LanguageId { get; set; }
    }

    #endregion
}
