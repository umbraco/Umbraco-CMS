using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;

namespace Umbraco.Cms.Api.Management.Mapping.TemporaryFile;

public class TemporaryFileViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<UploadSingleFileRequestModel, TempFileModel>((source, context) => new TempFileModel(), Map);
        mapper.Define<TempFileModel, UploadSingleFileResponseModel>((source, context) => new UploadSingleFileResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -AvailableUntil
    private void Map(UploadSingleFileRequestModel source, TempFileModel target, MapperContext context)
    {
        target.DataStream = source.File.OpenReadStream();
        target.FileName = source.File.FileName;
        target.Key = source.Key;
    }

    // Umbraco.Code.MapAll -DataStream
    private void Map(TempFileModel source, UploadSingleFileResponseModel target, MapperContext context)
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
