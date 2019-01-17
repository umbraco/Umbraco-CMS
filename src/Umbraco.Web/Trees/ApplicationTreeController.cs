using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [AngularJsonOnlyConfiguration]
    [PluginController("UmbracoTrees")]
    public class ApplicationTreeController : UmbracoAuthorizedApiController
    {        
        /// <summary>
        /// Returns the tree nodes for an application
        /// </summary>
        /// <param name="application">The application to load tree for</param>
        /// <param name="tree">An optional single tree alias, if specified will only load the single tree for the request app</param>
        /// <param name="queryStrings"></param>
        /// <param name="onlyInitialized">An optional bool (defaults to true), if set to false it will also load uninitialized trees</param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public async Task<TreeRootNode> GetApplicationTrees(string application, string tree, FormDataCollection queryStrings, bool onlyInitialized = true)
        {
            application = application.CleanForXss();

            if (string.IsNullOrEmpty(application)) throw new HttpResponseException(HttpStatusCode.NotFound);

            //find all tree definitions that have the current application alias
            var groupedTrees = Services.ApplicationTreeService.GetGroupedApplicationTrees(application, onlyInitialized);
            var allTrees = groupedTrees.Values.SelectMany(x => x).ToList();

            if (string.IsNullOrEmpty(tree) == false || allTrees.Count == 1)
            {
                var apptree = !tree.IsNullOrWhiteSpace()
                    ? allTrees.FirstOrDefault(x => x.Alias == tree)
                    : allTrees.FirstOrDefault();

                if (apptree == null) throw new HttpResponseException(HttpStatusCode.NotFound);

                var result = await GetRootForSingleAppTree(
                    apptree,
                    Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                    queryStrings,
                    application);

                //this will be null if it cannot convert to a single root section
                if (result != null)
                {
                    return result;
                }
            }

            //Don't apply fancy grouping logic futher down, if we only have one group of items
            var hasGroups = groupedTrees.Count > 1;
            if (!hasGroups)
            {
                var collection = new TreeNodeCollection();
                foreach (var apptree in allTrees)
                {
                    //return the root nodes for each tree in the app
                    var rootNode = await GetRootForMultipleAppTree(apptree, queryStrings);
                    //This could be null if the tree decides not to return it's root (i.e. the member type tree does this when not in umbraco membership mode)
                    if (rootNode != null)
                    {
                        collection.Add(rootNode);
                    }
                }

                if(collection.Count > 0)
                {
                    var multiTree = TreeRootNode.CreateMultiTreeRoot(collection);
                    multiTree.Name = Services.TextService.Localize("sections/" + application);

                    return multiTree;
                }

                //Otherwise its a application/section with no trees (aka a full screen app)
                //For example we do not have a Forms tree defined in C# & can not attribute with [Tree(isSingleNodeTree:true0]
                var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
                var section = Services.TextService.Localize("sections/" + application);

                return TreeRootNode.CreateSingleTreeRoot(rootId, null, null, section, TreeNodeCollection.Empty, true);
            }

            var rootNodeGroups = new List<TreeRootNode>();

            //Group trees by [CoreTree] attribute with a TreeGroup property
            foreach (var treeSectionGroup in groupedTrees)
            {
                var treeGroupName = treeSectionGroup.Key;

                var groupNodeCollection = new TreeNodeCollection();
                foreach (var appTree in treeSectionGroup.Value)
                {
                    var rootNode = await GetRootForMultipleAppTree(appTree, queryStrings);
                    if (rootNode != null)
                    {
                        //Add to a new list/collection
                        groupNodeCollection.Add(rootNode);
                    }
                }

                //If treeGroupName == null then its third party
                if (treeGroupName.IsNullOrWhiteSpace())
                {
                    //This is used for the localisation key
                    //treeHeaders/thirdPartyGroup
                    treeGroupName = "thirdPartyGroup";
                }

                if (groupNodeCollection.Count > 0)
                {
                    var groupRoot = TreeRootNode.CreateGroupNode(groupNodeCollection, application);
                    groupRoot.Name = Services.TextService.Localize("treeHeaders/" + treeGroupName);

                    rootNodeGroups.Add(groupRoot);
                }
            }

            return TreeRootNode.CreateGroupedMultiTreeRoot(new TreeNodeCollection(rootNodeGroups.OrderBy(x => x.Name)));
        }

        /// <summary>
        /// Get the root node for an application with multiple trees
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private async Task<TreeNode> GetRootForMultipleAppTree(ApplicationTree configTree, FormDataCollection queryStrings)
        {
            if (configTree == null) throw new ArgumentNullException(nameof(configTree));
            try
            {
                var byControllerAttempt = await configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext);
                if (byControllerAttempt.Success)
                {
                    return byControllerAttempt.Result;
                }
            }
            catch (HttpResponseException)
            {
                //if this occurs its because the user isn't authorized to view that tree, in this case since we are loading multiple trees we
                //will just return null so that it's not added to the list.
                return null;
            }

            throw new ApplicationException("Could not get root node for tree type " + configTree.Alias);
        }

        /// <summary>
        /// Get the root node for an application with one tree
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        private async Task<TreeRootNode> GetRootForSingleAppTree(ApplicationTree configTree, string id, FormDataCollection queryStrings, string application)
        {
            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
            if (configTree == null) throw new ArgumentNullException(nameof(configTree));
            var byControllerAttempt = configTree.TryLoadFromControllerTree(id, queryStrings, ControllerContext);
            if (byControllerAttempt.Success)
            {
                var rootNode = await configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext);
                if (rootNode.Success == false)
                {
                    //This should really never happen if we've successfully got the children above.
                    throw new InvalidOperationException("Could not create root node for tree " + configTree.Alias);
                }

                var treeAttribute = configTree.GetTreeAttribute();

                var sectionRoot = TreeRootNode.CreateSingleTreeRoot(
                    rootId,
                    rootNode.Result.ChildNodesUrl,
                    rootNode.Result.MenuUrl,
                    rootNode.Result.Name,
                    byControllerAttempt.Result,
                    treeAttribute.IsSingleNodeTree);

                //assign the route path based on the root node, this means it will route there when the section is navigated to
                //and no dashboards will be available for this section
                sectionRoot.RoutePath = rootNode.Result.RoutePath;

                foreach (var d in rootNode.Result.AdditionalData)
                {
                    sectionRoot.AdditionalData[d.Key] = d.Value;
                }
                return sectionRoot;

            }

            throw new ApplicationException("Could not render a tree for type " + configTree.Alias);
        }
    }
}
