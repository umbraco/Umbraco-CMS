using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Mapping.TemporaryFile;

public class TemporaryFileViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<CreateTemporaryFileRequestModel, TemporaryFileModel>((source, context) => new TemporaryFileModel(), Map);
        mapper.Define<TemporaryFileModel, CreateTemporaryFileResponseModel>((source, context) => new CreateTemporaryFileResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -AvailableUntil
    private void Map(CreateTemporaryFileRequestModel source, TemporaryFileModel target, MapperContext context)
    {
        target.DataStream = source.File.OpenReadStream();
        target.FileName = source.File.FileName;
        target.Key = source.Key;
    }

    // Umbraco.Code.MapAll -DataStream
    private void Map(Core.Models.TemporaryFile.TemporaryFileModel source, CreateTemporaryFileResponseModel target, MapperContext context)
    {
        if (source.AvailableUntil.HasValue == false)
        {
            throw new NotSupportedException("Mapping only allowed when AvailableUntil is set");
        }

        target.Key = source.Key;
        target.AvailableUntil = source.AvailableUntil.Value;
        target.FileName = source.FileName;
    }

}
