using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class ByKeyStartNodeOriginFinder : IStartNodeOriginFinder
{
    protected virtual string SupportedOriginType { get; set; } = "ByKey";
    private readonly IEntityService _entityService;

    public ByKeyStartNodeOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    public virtual Guid? FindOriginKey(StartNodeSelector selector)
    {
        if (selector.OriginAlias != SupportedOriginType || selector.OriginKey is null)
        {
            return null;
        }

        IEntitySlim? entity = _entityService.Get(selector.OriginKey.Value);

        if (entity is null
            || entity.NodeObjectType != Constants.ObjectTypes.Document
           )
        {
            return null;
        }


        return entity.Key;
    }
}
