using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// This is used to output JSON from legacy trees
    /// </summary>
    [PluginController("UmbracoTrees"), LegacyTreeAuthorizeAttribute]
    public class LegacyTreeController : TreeControllerBase
    {
        private readonly XmlTreeNode _xmlTreeNode;
        private readonly string _currentSection;

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            return LegacyTreeDataConverter.ConvertFromLegacy(
                _xmlTreeNode.NodeID,
                _xmlTreeNode,
                Url,
                _currentSection,
                queryStrings,
                isRoot: true);
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = GetTree(queryStrings);
            var attempt = tree.TryLoadFromLegacyTree(id, queryStrings, Url, tree.ApplicationAlias);
            if (attempt.Success == false)
            {
                Logger.Error<LegacyTreeController>(attempt.Exception, "Could not render tree {TreeType} for node id {NodeId}", queryStrings.GetRequiredString("treeType"), id);
                throw new ApplicationException("Could not render tree " + queryStrings.GetRequiredString("treeType") + " for node id " + id);
            }

            return attempt.Result;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //get the parent id from the query strings
            var parentId = queryStrings.GetRequiredString("parentId");
            var tree = GetTree(queryStrings);

            var rootIds = new[]
            {
                Core.Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                Core.Constants.System.RecycleBinContent.ToString(CultureInfo.InvariantCulture),
                Core.Constants.System.RecycleBinMedia.ToString(CultureInfo.InvariantCulture)
            };

            //if the id and the parentId are both -1 then we need to get the menu for the root node
            if (rootIds.Contains(id) && parentId == "-1")
            {
                var attempt = tree.TryGetMenuFromLegacyTreeRootNode(queryStrings, Url);
                if (attempt.Success == false)
                {
                    Logger.Error<LegacyTreeController>(attempt.Exception, "Could not render menu for root node for treeType {TreeType}", queryStrings.GetRequiredString("treeType"));
                    throw new ApplicationException("Could not render menu for root node for treeType " + queryStrings.GetRequiredString("treeType"));
                }

                foreach (var menuItem in attempt.Result.Items)
                {
                    menuItem.Name = Services.TextService.Localize("actions", menuItem.Alias);
                }
                return attempt.Result;
            }
            else
            {
                var attempt = tree.TryGetMenuFromLegacyTreeNode(parentId, id, queryStrings, Url);
                if (attempt.Success == false)
                {
                    Logger.Error<LegacyTreeController>(attempt.Exception, "Could not render menu for treeType {TreeType} for node id {ParentNodeId}", queryStrings.GetRequiredString("treeType"), parentId);
                    throw new ApplicationException("Could not render menu for treeType " + queryStrings.GetRequiredString("treeType") + " for node id " + parentId);
                }
                foreach (var menuItem in attempt.Result.Items)
                {
                    menuItem.Name = Services.TextService.Localize("actions", menuItem.Alias);
                }
                return attempt.Result;
            }
        }

        public override string RootNodeDisplayName { get; }

        public override string TreeAlias { get; }

        private ApplicationTree GetTree(FormDataCollection queryStrings)
        {
            //need to ensure we have a tree type
            var treeType = queryStrings.GetRequiredString("treeType");
            //now we'll look up that tree
            var tree = Services.ApplicationTreeService.GetByAlias(treeType);
            if (tree == null)
                throw new InvalidOperationException("No tree found with alias " + treeType);
            return tree;
        }

    }
}
