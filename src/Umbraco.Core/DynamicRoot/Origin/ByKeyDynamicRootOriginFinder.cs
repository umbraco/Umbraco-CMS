using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that locates a content item by its unique key.
///     This finder handles the "ByKey" origin type and validates that the entity exists and is of an allowed type.
/// </summary>
public class ByKeyDynamicRootOriginFinder : IDynamicRootOriginFinder
{
    /// <summary>
    ///     Gets or sets the origin type alias that this finder supports.
    /// </summary>
    protected virtual string SupportedOriginType { get; set; } = "ByKey";

    private readonly IEntityService _entityService;

    private ISet<Guid> _allowedObjectTypes = new HashSet<Guid>(new[]
    {
        Constants.ObjectTypes.Document, Constants.ObjectTypes.SystemRoot
    });

    /// <summary>
    ///     Initializes a new instance of the <see cref="ByKeyDynamicRootOriginFinder"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used to retrieve entities by key.</param>
    public ByKeyDynamicRootOriginFinder(IEntityService entityService)
    {
        _entityService = entityService;
    }

    /// <inheritdoc/>
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
