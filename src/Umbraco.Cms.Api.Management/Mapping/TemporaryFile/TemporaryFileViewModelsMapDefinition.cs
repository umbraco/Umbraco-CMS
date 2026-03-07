using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Mapping.TemporaryFile;

    /// <summary>
    /// Defines the mapping configuration between domain models and view models for temporary files in the API.
    /// </summary>
public class TemporaryFileViewModelsMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures the object mappings for temporary file-related view models using the specified mapper.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance on which to define the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<CreateTemporaryFileRequestModel, CreateTemporaryFileModel>((source, context) => new CreateTemporaryFileModel { FileName = string.Empty }, Map);
        mapper.Define<TemporaryFileModel, TemporaryFileResponseModel>((source, context) => new TemporaryFileResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(CreateTemporaryFileRequestModel source, CreateTemporaryFileModel target, MapperContext context)
    {
        target.OpenReadStream = () => source.File.OpenReadStream();
        target.FileName = source.File.FileName;
        target.Key = source.Id;
    }

    // Umbraco.Code.MapAll
    private void Map(TemporaryFileModel source, TemporaryFileResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.AvailableUntil = source.AvailableUntil;
        target.FileName = source.FileName;
    }
}
