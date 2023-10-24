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

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

    protected virtual string SupportedOriginType { get; set; } = "Root";
    public virtual Guid? FindOriginKey(StartNodeSelector selector)
    {
        if (selector.OriginAlias != SupportedOriginType)
        {
            return null;
        }
        var entity = _entityService.Get(selector.Context.ParentKey);

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
