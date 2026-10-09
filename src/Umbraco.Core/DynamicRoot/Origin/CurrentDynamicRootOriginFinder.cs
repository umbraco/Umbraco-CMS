using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that uses the current content item from the context as the origin.
///     This finder handles the "Current" origin type and extends <see cref="ByKeyDynamicRootOriginFinder"/>.
/// </summary>
public class CurrentDynamicRootOriginFinder : ByKeyDynamicRootOriginFinder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentDynamicRootOriginFinder"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used to retrieve entities by key.</param>
    public CurrentDynamicRootOriginFinder(IEntityService entityService)
        : base(entityService)
    {
    }

    /// <inheritdoc/>
    protected override string SupportedOriginType { get; set; } = "Current";

    /// <inheritdoc/>
    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        // When creating new content, CurrentKey will be null - fallback to using ParentKey.
        query.OriginKey = query.Context.CurrentKey ?? query.Context.ParentKey;
        return base.FindOriginKey(query);
    }
}
