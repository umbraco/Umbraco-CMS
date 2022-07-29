using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_2_0;

public class TablesIndexesFixup : MigrationBase
{
    public TablesIndexesFixup(IMigrationContext context) : base(context)
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

        var contentVersionCultureVariationVersionIdIndex = $"IX_{ContentVersionCultureVariationDto.TableName}";
        DeleteIndex<ContentVersionCultureVariationDto>(contentVersionCultureVariationVersionIdIndex);
        CreateIndex<ContentVersionCultureVariationDto>(contentVersionCultureVariationVersionIdIndex);

        var contentVersionDtoNodeIdV2Index = $"IX_{ContentVersionDto.TableName}_NodeIdV2";
        DeleteIndex<ContentVersionDto>(contentVersionDtoNodeIdV2Index);
        CreateIndex<ContentVersionDto>(contentVersionDtoNodeIdV2Index);
    }
}
