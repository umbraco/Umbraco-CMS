using Umbraco.Cms.Api.Management.ViewModels.Import;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ImportExport;

namespace Umbraco.Cms.Api.Management.Mapping.Import;

/// <summary>
/// Provides mapping configuration for converting entity import analysis data to corresponding view models within the import process.
/// </summary>
public class EntityImportAnalysisViewModelsMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mappings for converting <see cref="EntityXmlAnalysis"/> to <see cref="EntityImportAnalysisResponseModel"/> within the entity import analysis feature.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to register the mapping definitions.</param>
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
