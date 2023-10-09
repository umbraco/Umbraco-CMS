using Umbraco.Cms.Api.Management.ViewModels.Abstract;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Mapping.Abstract;

public class TracksTrashingMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) =>
        mapper.Define<ITreeEntity, ITracksTrashing>(Map);

    private void Map(ITreeEntity source, ITracksTrashing target, MapperContext context) => target.IsTrashed = source.Trashed;
}
