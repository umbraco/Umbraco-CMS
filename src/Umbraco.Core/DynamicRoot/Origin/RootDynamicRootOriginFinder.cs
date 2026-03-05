using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that locates the content tree root (first content item below the system root) for the current or parent content.
///     This finder handles the "Root" origin type and traverses the content path to find the topmost document node.
/// </summary>
public class RootDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    private readonly IEntityService _entityService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RootDynamicRootOriginFinder"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used to retrieve entities and traverse the content tree.</param>
    public RootDynamicRootOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

    /// <summary>
    ///     Gets or sets the origin type alias that this finder supports.
    /// </summary>
    protected virtual string SupportedOriginType { get; set; } = "Root";

    /// <inheritdoc/>
    public virtual Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        if (query.OriginAlias != SupportedOriginType)
        {
            return null;
        }

        // when creating new content, CurrentKey will be null - fallback to using ParentKey
        Guid entityKey = query.Context.CurrentKey ?? query.Context.ParentKey;
        var entity = _entityService.Get(entityKey);

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

    /// <summary>
    ///     Gets the root content ID from the path, skipping the system root and recycle bin.
    /// </summary>
    /// <param name="path">The path segments as an array of ID strings.</param>
    /// <returns>The root content ID, or <c>null</c> if no valid root is found.</returns>
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
