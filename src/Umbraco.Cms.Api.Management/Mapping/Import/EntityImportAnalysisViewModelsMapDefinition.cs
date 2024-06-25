using Umbraco.Cms.Api.Management.ViewModels.Import;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ImportExport;

namespace Umbraco.Cms.Api.Management.Mapping.Import;

public class EntityImportAnalysisViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<EntityXmlAnalysis, EntityImportAnalysisResponseModel>((_, _) => new EntityImportAnalysisResponseModel(), Map);
    }

    private void Map(EntityXmlAnalysis source, EntityImportAnalysisResponseModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Alias = source.Alias;
        target.EntityType = source.EntityType.ToString();
    }
}
