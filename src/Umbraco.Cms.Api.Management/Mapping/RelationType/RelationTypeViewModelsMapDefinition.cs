using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.RelationType;

public class RelationTypeViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IRelationType, RelationTypeResponseModel>((_, _) => new RelationTypeResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IRelationType source, RelationTypeResponseModel target, MapperContext context)
    {
        target.IsBidirectional = source.IsBidirectional;

        if (source is IRelationTypeWithIsDependency sourceWithIsDependency)
        {
            target.IsDependency = sourceWithIsDependency.IsDependency;
        }

        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Alias = source.Alias;

        target.ChildObject = MapObjectType(source.ChildObjectType);

        target.ParentObject = MapObjectType(source.ParentObjectType);
    }

    private ObjectTypeResponseModel? MapObjectType(Guid? objectTypeId)
        => objectTypeId is not null
            ? new ObjectTypeResponseModel
            {
                Id = objectTypeId.Value,
                Name = ObjectTypes.GetUmbracoObjectType(objectTypeId.Value).GetFriendlyName(),
            }
            : null;
}
