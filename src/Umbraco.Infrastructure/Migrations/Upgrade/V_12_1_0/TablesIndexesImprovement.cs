using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_1_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class TablesIndexesImprovement : MigrationBase
{
    public TablesIndexesImprovement(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        var nodeDtoTrashedIndex = $"IX_{NodeDto.TableName}_ObjectType_trashed_sorted";
        DeleteIndex<NodeDto>(nodeDtoTrashedIndex);
        CreateIndex<NodeDto>(nodeDtoTrashedIndex);

        var redirectUrlCreateDateUtcIndex = $"IX_{RedirectUrlDto.TableName}_culture_hash";
        DeleteIndex<RedirectUrlDto>(redirectUrlCreateDateUtcIndex);
        CreateIndex<RedirectUrlDto>(redirectUrlCreateDateUtcIndex);

        var contentVersionCultureVariationVersionIdIndex = $"IX_{ContentVersionCultureVariationDto.TableName}_VersionId";
        DeleteIndex<ContentVersionCultureVariationDto>(contentVersionCultureVariationVersionIdIndex);
        CreateIndex<ContentVersionCultureVariationDto>(contentVersionCultureVariationVersionIdIndex);

        var contentVersionDtoNodeIdV2Index = $"IX_{ContentVersionDto.TableName}_NodeId";
        DeleteIndex<ContentVersionDto>(contentVersionDtoNodeIdV2Index);
        CreateIndex<ContentVersionDto>(contentVersionDtoNodeIdV2Index);

        var tagRelationshipDtoTagNodeIndex = $"IX_{TagRelationshipDto.TableName}_tagId_nodeId";
        DeleteIndex<TagRelationshipDto>(tagRelationshipDtoTagNodeIndex);
        CreateIndex<TagRelationshipDto>(tagRelationshipDtoTagNodeIndex);

        var tagDtoLanguageGroupIndex = $"IX_{TagDto.TableName}_languageId_group";
        DeleteIndex<TagDto>(tagDtoLanguageGroupIndex);
        CreateIndex<TagDto>(tagDtoLanguageGroupIndex);

        var documentVersionDtoIdPublishedIndex = $"IX_{DocumentVersionDto.TableName}_id_published";
        DeleteIndex<DocumentVersionDto>(documentVersionDtoIdPublishedIndex);
        CreateIndex<DocumentVersionDto>(documentVersionDtoIdPublishedIndex);

        var documentVersionDtoPublishedIndex = $"IX_{DocumentVersionDto.TableName}_published";
        DeleteIndex<DocumentVersionDto>(documentVersionDtoPublishedIndex);
        CreateIndex<DocumentVersionDto>(documentVersionDtoPublishedIndex);

        var logDtoDatestampIndex = $"IX_{LogDto.TableName}_datestamp";
        DeleteIndex<LogDto>(logDtoDatestampIndex);
        CreateIndex<LogDto>(logDtoDatestampIndex);

        var logDtoDatestampHeaderIndex = $"IX_{LogDto.TableName}_datestamp_logheader";
        DeleteIndex<LogDto>(logDtoDatestampHeaderIndex);
        CreateIndex<LogDto>(logDtoDatestampHeaderIndex);

        var propertyDataDtoVersionIdIndex = $"IX_{PropertyDataDto.TableName}_VersionId";
        DeleteIndex<PropertyDataDto>(propertyDataDtoVersionIdIndex);
        CreateIndex<PropertyDataDto>(propertyDataDtoVersionIdIndex);

        var contentNuDtoPublishedIdIndex = $"IX_{ContentNuDto.TableName}_published";
        DeleteIndex<ContentNuDto>(contentNuDtoPublishedIdIndex);
        CreateIndex<ContentNuDto>(contentNuDtoPublishedIdIndex);

        var nodeDtoParentIdNodeObjectTypeIndex = $"IX_{NodeDto.TableName}_parentId_nodeObjectType";
        DeleteIndex<NodeDto>(nodeDtoParentIdNodeObjectTypeIndex);
        CreateIndex<NodeDto>(nodeDtoParentIdNodeObjectTypeIndex);
    }
}
