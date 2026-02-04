using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
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
public abstract class PublishableContentRepositoryBase<TId, TEntity, TRepository, TEntityDto, TContentVersionDto>
    : ContentRepositoryBase<TId, TEntity, TRepository>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto
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
