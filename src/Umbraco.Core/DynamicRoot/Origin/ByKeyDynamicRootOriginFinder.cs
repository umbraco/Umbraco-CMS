using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class ByKeyDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    protected virtual string SupportedOriginType { get; set; } = "ByKey";

    private readonly IEntityService _entityService;

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

    public ByKeyDynamicRootOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    public virtual Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType || query.OriginKey is null)
        {
            return null;
        }

        IEntitySlim? entity = _entityService.Get(query.OriginKey.Value);

        if (entity is null || _allowedObjectTypes.Contains(entity.NodeObjectType) is false)
        {
            return null;
        }

        return entity.Key;
    }
}
