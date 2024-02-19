using Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.DynamicRoot;

/// <inheritdoc />
public class DynamicRootMapDefinition : IMapDefinition
{
    /// <inheritdoc/>
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<DynamicRootRequestModel, DynamicRootNodeQuery>((source, context) => new DynamicRootNodeQuery { OriginAlias = null!, Context = default }, Map);

    // Umbraco.Code.MapAll
    private static void Map(DynamicRootRequestModel source, DynamicRootNodeQuery target, MapperContext context)
    {
        target.Context = new DynamicRootContext()
        {
            CurrentKey = source.Context.Id,
            ParentKey = source.Context.Parent.Id
        };
        target.OriginKey = source.Query.Origin.Id;
        target.OriginAlias = source.Query.Origin.Alias;
        target.QuerySteps = source.Query.Steps.Select(x => new DynamicRootQueryStep()
        {
            Alias = x.Alias,
            AnyOfDocTypeKeys = x.DocumentTypeIds
        });
    }
}
