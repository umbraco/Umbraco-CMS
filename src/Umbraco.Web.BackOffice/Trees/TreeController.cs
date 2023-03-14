using System.Collections.Concurrent;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     The base controller for all tree requests
/// </summary>
public abstract class TreeController : TreeControllerBase
{
    private static readonly ConcurrentDictionary<Type, TreeAttribute> _treeAttributeCache = new();

    private readonly TreeAttribute _treeAttribute;

    protected TreeController(ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator)
        : base(umbracoApiControllerTypeCollection, eventAggregator)
    {
        LocalizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _treeAttribute = GetTreeAttribute();
    }

    protected ILocalizedTextService LocalizedTextService { get; }

    /// <inheritdoc />
    public override string? RootNodeDisplayName => Tree.GetRootNodeDisplayName(this, LocalizedTextService);

    /// <inheritdoc />
    public override string? TreeGroup => _treeAttribute.TreeGroup;

    /// <inheritdoc />
    public override string TreeAlias => _treeAttribute.TreeAlias;

    /// <inheritdoc />
    public override string? TreeTitle => _treeAttribute.TreeTitle;

    /// <inheritdoc />
    public override TreeUse TreeUse => _treeAttribute.TreeUse;

    /// <inheritdoc />
    public override string SectionAlias => _treeAttribute.SectionAlias;

    /// <inheritdoc />
    public override int SortOrder => _treeAttribute.SortOrder;

    /// <inheritdoc />
    public override bool IsSingleNodeTree => _treeAttribute.IsSingleNodeTree;

    private TreeAttribute GetTreeAttribute() =>
        _treeAttributeCache.GetOrAdd(GetType(), type =>
        {
            TreeAttribute? treeAttribute = type.GetCustomAttribute<TreeAttribute>(false);
            if (treeAttribute == null)
            {
                throw new InvalidOperationException("The Tree controller is missing the " +
                                                    typeof(TreeAttribute).FullName + " attribute");
            }

            return treeAttribute;
        });
}
