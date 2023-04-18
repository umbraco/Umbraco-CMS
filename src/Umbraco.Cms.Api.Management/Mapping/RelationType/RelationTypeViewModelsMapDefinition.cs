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
        target.ChildObjectType = source.ChildObjectType;
        target.IsBidirectional = source.IsBidirectional;

        if (source is IRelationTypeWithIsDependency sourceWithIsDependency)
        {
            target.IsDependency = sourceWithIsDependency.IsDependency;
        }

        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Alias = source.Alias;
        target.ParentObjectType = source.ParentObjectType;
        target.Path = "-1," + source.Id;

        target.IsSystemRelationType = source.IsSystemRelationType();

        // Set the "friendly" and entity names for the parent and child object types
        if (source.ParentObjectType.HasValue)
        {
            UmbracoObjectTypes objType = ObjectTypes.GetUmbracoObjectType(source.ParentObjectType.Value);
            target.ParentObjectTypeName = objType.GetFriendlyName();
        }

        if (source.ChildObjectType.HasValue)
        {
            UmbracoObjectTypes objType = ObjectTypes.GetUmbracoObjectType(source.ChildObjectType.Value);
            target.ChildObjectTypeName = objType.GetFriendlyName();
        }
    }
}
