using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.RelationType;

/// <summary>
/// Provides mapping configuration between <see cref="RelationType"/> entities and their corresponding view models within the API management layer.
/// </summary>
public class RelationTypeViewModelsMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures the object mappings between <see cref="IRelationType"/> domain models and <see cref="RelationTypeResponseModel"/> view models.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mappings.</param>
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
