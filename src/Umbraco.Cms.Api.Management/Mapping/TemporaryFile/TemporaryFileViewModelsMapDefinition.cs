using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Mapping.TemporaryFile;

public class TemporaryFileViewModelsMapDefinition : IMapDefinition
{
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
