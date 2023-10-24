using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class ByKeyStartNodeOriginFinder : IStartNodeOriginFinder
{
    protected virtual string SupportedOriginType { get; set; } = "ByKey";
    private readonly IEntityService _entityService;

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

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

        if (entity is null || _allowedObjectTypes.Contains(entity.NodeObjectType) is false)
        {
            return null;
        }


        return entity.Key;
    }
}
