using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IPublishableContentBase" />.
/// </summary>
internal abstract class PublishableContentRepositoryBase<TId, TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto>
    : ContentRepositoryBase<TId, TEntity, TRepository>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto
    where TContentCultureVariationDto : class, ICultureVariationDto
{
    protected PublishableContentRepositoryBase(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<ContentRepositoryBase<TId, TEntity, TRepository>> logger,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
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
    }

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

        if (entityVariations.TryGetValue(content.Id, out List<EntityVariation>? documentVariation))
        {
            content.SetCultureEdited(documentVariation.Where(x => x.Edited).Select(x => x.Culture));
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
                        r.Select(documentDto => documentDto.ContentDto, r1 =>
                                r1.Select(contentDto => contentDto.NodeDto))
                            .Select(documentDto => documentDto.ContentVersionDto, r1 =>
                                r1.Select(documentVersionDto => documentVersionDto.ContentVersionDto))
                            .Select(documentDto => documentDto.PublishedVersionDto, "pdv", r1 =>
                                r1.Select(documentVersionDto => documentVersionDto!.ContentVersionDto, "pcv")))

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
}
