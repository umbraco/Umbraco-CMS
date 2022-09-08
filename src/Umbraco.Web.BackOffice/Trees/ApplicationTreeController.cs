using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Extensions;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Web.Routing;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     Used to return tree root nodes
/// </summary>
[AngularJsonOnlyConfiguration]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
public class ApplicationTreeController : UmbracoAuthorizedApiController
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IControllerFactory _controllerFactory;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ISectionService _sectionService;
    private readonly ITreeService _treeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationTreeController" /> class.
    /// </summary>
    public ApplicationTreeController(
        ITreeService treeService,
        ISectionService sectionService,
        ILocalizedTextService localizedTextService,
        IControllerFactory controllerFactory,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _treeService = treeService;
        _sectionService = sectionService;
        _localizedTextService = localizedTextService;
        _controllerFactory = controllerFactory;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    /// <summary>
    ///     Returns the tree nodes for an application
    /// </summary>
    /// <param name="application">The application to load tree for</param>
    /// <param name="tree">An optional single tree alias, if specified will only load the single tree for the request app</param>
    /// <param name="queryStrings">The query strings</param>
    /// <param name="use">Tree use.</param>
    public async Task<ActionResult<TreeRootNode>> GetApplicationTrees(string? application, string? tree, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection? queryStrings, TreeUse use = TreeUse.Main)
    {
        application = application?.CleanForXss();

        if (string.IsNullOrEmpty(application))
        {
            return NotFound();
        }

        ISection? section = _sectionService.GetByAlias(application);
        if (section == null)
        {
            return NotFound();
        }

        // find all tree definitions that have the current application alias
        IDictionary<string, IEnumerable<Tree>> groupedTrees = _treeService.GetBySectionGrouped(application, use);
        var allTrees = groupedTrees.Values.SelectMany(x => x).ToList();

        if (allTrees.Count == 0)
        {
            // if there are no trees defined for this section but the section is defined then we can have a simple
            // full screen section without trees
            var name = _localizedTextService.Localize("sections", application);
            return TreeRootNode.CreateSingleTreeRoot(Constants.System.RootString, null, null, name, TreeNodeCollection.Empty, true);
        }

        // handle request for a specific tree / or when there is only one tree
        if (!tree.IsNullOrWhiteSpace() || allTrees.Count == 1)
        {
            Tree? t = tree.IsNullOrWhiteSpace()
                ? allTrees[0]
                : allTrees.FirstOrDefault(x => x.TreeAlias == tree);

            if (t == null)
            {
                return NotFound();
            }

            ActionResult<TreeRootNode>? treeRootNode = await GetTreeRootNode(t, Constants.System.Root, queryStrings);

            if (treeRootNode != null)
            {
                return treeRootNode;
            }

            return NotFound();
        }

        // handle requests for all trees
        // for only 1 group
        if (groupedTrees.Count == 1)
        {
            var nodes = new TreeNodeCollection();
            foreach (Tree t in allTrees)
            {
                ActionResult<TreeNode?>? nodeResult = await TryGetRootNode(t, queryStrings);
                if (!(nodeResult?.Result is null))
                {
                    return nodeResult.Result;
                }

                TreeNode? node = nodeResult?.Value;
                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            var name = _localizedTextService.Localize("sections", application);

            if (nodes.Count > 0)
            {
                var treeRootNode = TreeRootNode.CreateMultiTreeRoot(nodes);
                treeRootNode.Name = name;
                return treeRootNode;
            }

            // otherwise it's a section with all empty trees, aka a fullscreen section
            // todo is this true? what if we just failed to TryGetRootNode on all of them? SD: Yes it's true but we should check the result of TryGetRootNode and throw?
            return TreeRootNode.CreateSingleTreeRoot(Constants.System.RootString, null, null, name, TreeNodeCollection.Empty, true);
        }

        // for many groups
        var treeRootNodes = new List<TreeRootNode>();
        foreach ((var groupName, IEnumerable<Tree> trees) in groupedTrees)
        {
            var nodes = new TreeNodeCollection();
            foreach (Tree t in trees)
            {
                ActionResult<TreeNode?>? nodeResult = await TryGetRootNode(t, queryStrings);
                if (nodeResult != null && nodeResult.Result is not null)
                {
                    return nodeResult.Result;
                }

                TreeNode? node = nodeResult?.Value;

                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            if (nodes.Count == 0)
            {
                continue;
            }

            // no name => third party
            // use localization key treeHeaders/thirdPartyGroup
            // todo this is an odd convention
            var name = groupName.IsNullOrWhiteSpace() ? "thirdPartyGroup" : groupName;

            var groupRootNode = TreeRootNode.CreateGroupNode(nodes, application);
            groupRootNode.Name = _localizedTextService.Localize("treeHeaders", name);
            treeRootNodes.Add(groupRootNode);
        }

        return TreeRootNode.CreateGroupedMultiTreeRoot(new TreeNodeCollection(treeRootNodes.OrderBy(x => x.Name)));
    }

    /// <summary>
    ///     Tries to get the root node of a tree.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns null if the root node could not be obtained due to that
    ///         the user isn't authorized to view that tree. In this case since we are
    ///         loading multiple trees we will just return null so that it's not added
    ///         to the list
    ///     </para>
    /// </remarks>
    private async Task<ActionResult<TreeNode?>?> TryGetRootNode(Tree tree, FormCollection? querystring)
    {
        if (tree == null)
        {
            throw new ArgumentNullException(nameof(tree));
        }

        return await GetRootNode(tree, querystring);
    }

    /// <summary>
    ///     Get the tree root node of a tree.
    /// </summary>
    private async Task<ActionResult<TreeRootNode>> GetTreeRootNode(Tree tree, int id, FormCollection? querystring)
    {
        if (tree == null)
        {
            throw new ArgumentNullException(nameof(tree));
        }

        ActionResult<TreeNodeCollection?>? childrenResult = await GetChildren(tree, id, querystring);
        if (!(childrenResult?.Result is null))
        {
            return new ActionResult<TreeRootNode>(childrenResult.Result);
        }

        TreeNodeCollection? children = childrenResult?.Value;
        ActionResult<TreeNode?>? rootNodeResult = await GetRootNode(tree, querystring);
        if (!(rootNodeResult?.Result is null))
        {
            return rootNodeResult.Result;
        }

        TreeNode? rootNode = rootNodeResult?.Value;


        var sectionRoot = TreeRootNode.CreateSingleTreeRoot(
            Constants.System.RootString,
            rootNode!.ChildNodesUrl,
            rootNode.MenuUrl,
            rootNode.Name,
            children,
            tree.IsSingleNodeTree);

        // assign the route path based on the root node, this means it will route there when the
        // section is navigated to and no dashboards will be available for this section
        sectionRoot.RoutePath = rootNode.RoutePath;
        sectionRoot.Path = rootNode.Path;

        foreach (KeyValuePair<string, object?> d in rootNode.AdditionalData)
        {
            sectionRoot.AdditionalData[d.Key] = d.Value;
        }

        return sectionRoot;
    }

    /// <summary>
    ///     Gets the root node of a tree.
    /// </summary>
    private async Task<ActionResult<TreeNode?>?> GetRootNode(Tree tree, FormCollection? querystring)
    {
        if (tree == null)
        {
            throw new ArgumentNullException(nameof(tree));
        }

        ActionResult<object> result = await GetApiControllerProxy(tree.TreeControllerType, "GetRootNode", querystring);

        // return null if the user isn't authorized to view that tree
        if (!((ForbidResult?)result.Result is null))
        {
            return null;
        }

        var controller = (TreeControllerBase?)result.Value;
        TreeNode? rootNode = null;
        if (controller is not null)
        {
            ActionResult<TreeNode?> rootNodeResult = await controller.GetRootNode(querystring);
            if (!(rootNodeResult.Result is null))
            {
                return rootNodeResult.Result;
            }

            rootNode = rootNodeResult.Value;

            if (rootNode == null)
            {
                throw new InvalidOperationException($"Failed to get root node for tree \"{tree.TreeAlias}\".");
            }
        }

        return rootNode;
    }

    /// <summary>
    ///     Get the child nodes of a tree node.
    /// </summary>
    private async Task<ActionResult<TreeNodeCollection?>?> GetChildren(Tree tree, int id, FormCollection? querystring)
    {
        if (tree == null)
        {
            throw new ArgumentNullException(nameof(tree));
        }

        // the method we proxy has an 'id' parameter which is *not* in the querystring,
        // we need to add it for the proxy to work (else, it does not find the method,
        // when trying to run auth filters etc).
        Dictionary<string, StringValues> d = querystring?.ToDictionary(x => x.Key, x => x.Value) ??
                                             new Dictionary<string, StringValues>();
        d["id"] = StringValues.Empty;
        var proxyQuerystring = new FormCollection(d);

        ActionResult<object> controllerResult =
            await GetApiControllerProxy(tree.TreeControllerType, "GetNodes", proxyQuerystring);
        if (!(controllerResult.Result is null))
        {
            return new ActionResult<TreeNodeCollection?>(controllerResult.Result);
        }

        var controller = (TreeControllerBase?)controllerResult.Value;
        return controller is not null ? await controller.GetNodes(id.ToInvariantString(), querystring) : null;
    }

    /// <summary>
    ///     Gets a proxy to a controller for a specified action.
    /// </summary>
    /// <param name="controllerType">The type of the controller.</param>
    /// <param name="action">The action.</param>
    /// <param name="querystring">The querystring.</param>
    /// <returns>An instance of the controller.</returns>
    /// <remarks>
    ///     <para>
    ///         Creates an instance of the <paramref name="controllerType" /> and initializes it with a route
    ///         and context etc. so it can execute the specified <paramref name="action" />. Runs the authorization
    ///         filters for that action, to ensure that the user has permission to execute it.
    ///     </para>
    /// </remarks>
    private async Task<ActionResult<object>> GetApiControllerProxy(Type controllerType, string action, FormCollection? querystring)
    {
        // note: this is all required in order to execute the auth-filters for the sub request, we
        // need to "trick" mvc into thinking that it is actually executing the proxied controller.

        var controllerName = ControllerExtensions.GetControllerName(controllerType);

        // create proxy route data specifying the action & controller to execute
        var routeData = new RouteData(new RouteValueDictionary
        {
            [ActionToken] = action,
            [ControllerToken] = controllerName
        });
        if (!(querystring is null))
        {
            foreach ((var key, StringValues value) in querystring)
            {
                routeData.Values[key] = value;
            }
        }

        ControllerActionDescriptor? actionDescriptor = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .Cast<ControllerActionDescriptor>()
            .First(x =>
                x.ControllerName.Equals(controllerName) &&
                x.ActionName == action);

        var actionContext = new ActionContext(HttpContext, routeData, actionDescriptor);
        var proxyControllerContext = new ControllerContext(actionContext);
        var controller = (TreeControllerBase)_controllerFactory.CreateController(proxyControllerContext);

        // TODO: What about other filters? Will they execute?
        var isAllowed = await controller.ControllerContext.InvokeAuthorizationFiltersForRequest(actionContext);
        if (!isAllowed)
        {
            return Forbid();
        }

        return controller;
    }
}
