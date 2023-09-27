using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class RootStartNodeOriginFinder : IStartNodeOriginFinder
{
    private readonly IEntityService _entityService;

    public RootStartNodeOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    protected virtual string SupportedOriginType { get; set; } = "Root";
    public virtual Guid? FindOriginKey(StartNodeSelector selector)
    {
        if (selector.OriginAlias != SupportedOriginType || selector.Context.CurrentKey.HasValue is false)
        {
            return null;
        }
        var entity = _entityService.Get(selector.Context.CurrentKey.Value);

        if (entity is null
            || entity.NodeObjectType != Constants.ObjectTypes.Document
           )
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
            || root.NodeObjectType != Constants.ObjectTypes.Document
           )
        {
            return null;
        }

        return root.Key;
    }

    private int? GetRootId(string[] path)
    {
        foreach (var contentId in path)
        {
            if (contentId == Constants.System.RootString
                || contentId == Constants.System.RecycleBinContentString)
            {
                continue;
            }

            return int.Parse(contentId, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        return null;
    }
}
