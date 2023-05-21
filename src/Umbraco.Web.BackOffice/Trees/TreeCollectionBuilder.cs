using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     Builds a <see cref="TreeCollection" />.
/// </summary>
public class TreeCollectionBuilder : ICollectionBuilder<TreeCollection, Tree>
{
    private readonly List<Tree> _trees = new();

    public TreeCollection CreateCollection(IServiceProvider factory) => new(() => _trees);

    public void RegisterWith(IServiceCollection services) =>
        services.Add(new ServiceDescriptor(typeof(TreeCollection), CreateCollection, ServiceLifetime.Singleton));


    /// <summary>
    ///     Registers a custom tree definition
    /// </summary>
    /// <param name="treeDefinition"></param>
    /// <remarks>
    ///     This is useful if a developer wishes to have a single tree controller for different tree aliases. In this case the
    ///     tree controller
    ///     cannot be decorated with the TreeAttribute (since then it will be auto-registered).
    /// </remarks>
    public void AddTree(Tree treeDefinition)
    {
        if (treeDefinition == null)
        {
            throw new ArgumentNullException(nameof(treeDefinition));
        }

        _trees.Add(treeDefinition);
    }

    public void AddTreeController<TController>()
        where TController : TreeControllerBase
        => AddTreeController(typeof(TController));

    public void AddTreeController(Type controllerType)
    {
        if (!typeof(TreeControllerBase).IsAssignableFrom(controllerType))
        {
            throw new ArgumentException(
                $"Type {controllerType} does not inherit from {typeof(TreeControllerBase).FullName}.");
        }

        // not all TreeControllerBase are meant to be used here,
        // ignore those that don't have the attribute

        TreeAttribute? attribute = controllerType.GetCustomAttribute<TreeAttribute>(false);
        if (attribute == null)
        {
            return;
        }

        var isCoreTree = controllerType.HasCustomAttribute<CoreTreeAttribute>(false);

        // Use section as tree group if core tree, so it isn't grouped by empty key and thus end up in "Third Party" tree group if adding custom tree nodes in other groups, e.g. "Settings" tree group.
        attribute.TreeGroup ??= isCoreTree ? attribute.SectionAlias : attribute.TreeGroup;

        var tree = new Tree(
            attribute.SortOrder,
            attribute.SectionAlias,
            attribute.TreeGroup,
            attribute.TreeAlias,
            attribute.TreeTitle,
            attribute.TreeUse,
            controllerType,
            attribute.IsSingleNodeTree);
        _trees.Add(tree);
    }

    public void AddTreeControllers(IEnumerable<Type> controllerTypes)
    {
        foreach (Type controllerType in controllerTypes)
        {
            AddTreeController(controllerType);
        }
    }

    public void RemoveTree(Tree treeDefinition)
    {
        if (treeDefinition == null)
        {
            throw new ArgumentNullException(nameof(treeDefinition));
        }

        _trees.Remove(treeDefinition);
    }

    public void RemoveTreeController<T>()
        where T : TreeControllerBase
        => RemoveTreeController(typeof(T));

    // TODO: Change parameter name to "controllerType" in a major version to make it consistent with AddTreeController method.
    public void RemoveTreeController(Type type)
    {
        if (!typeof(TreeControllerBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type} does not inherit from {typeof(TreeControllerBase).FullName}.");
        }

        Tree? tree = _trees.FirstOrDefault(x => x.TreeControllerType == type);
        if (tree != null)
        {
            _trees.Remove(tree);
        }
    }

    public void RemoveTreeControllers(IEnumerable<Type> controllerTypes)
    {
        foreach (Type controllerType in controllerTypes)
        {
            RemoveTreeController(controllerType);
        }
    }
}
