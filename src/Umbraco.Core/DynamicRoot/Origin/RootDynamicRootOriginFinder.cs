using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class RootDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    private readonly IEntityService _entityService;

    public RootDynamicRootOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

    protected virtual string SupportedOriginType { get; set; } = "Root";

    public virtual Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType)
        {
            return null;
        }

        var entity = _entityService.Get(query.Context.ParentKey);

        if (entity is null || _allowedObjectTypes.Contains(entity.NodeObjectType) is false)
        {
            return null;
        }

        var path = entity.Path.Split(",");
        if (path.Length < 2)
        {
            return null;
        }


        var rootId = GetRootId(path);
        IEntitySlim? root = rootId is null ? null : _entityService.Get(rootId.Value);

        if (root is null
            || root.NodeObjectType != Constants.ObjectTypes.Document)
        {
            return null;
        }

        return root.Key;
    }

    private static int? GetRootId(string[] path)
    {
        foreach (var contentId in path)
        {
            if (contentId is Constants.System.RootString or Constants.System.RecycleBinContentString)
            {
                continue;
            }

            return int.Parse(contentId, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        return null;
    }
}
