using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     An origin finder that uses the parent content item from the context as the origin.
///     This finder handles the "Parent" origin type and extends <see cref="ByKeyDynamicRootOriginFinder"/>.
/// </summary>
public class ParentDynamicRootOriginFinder : ByKeyDynamicRootOriginFinder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ParentDynamicRootOriginFinder"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used to retrieve entities by key.</param>
    public ParentDynamicRootOriginFinder(IEntityService entityService) : base(entityService)
    {
    }

    /// <inheritdoc/>
    protected override string SupportedOriginType { get; set; } = "Parent";

    /// <inheritdoc/>
    public override Guid? FindOriginKey(DynamicRootNodeQuery query)
    {
        query.OriginKey = query.Context.ParentKey;
        var baseResult = base.FindOriginKey(query);

        return baseResult;
    }
}
