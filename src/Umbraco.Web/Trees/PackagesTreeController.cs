using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Packages)]
    [Tree(Constants.Applications.Developer, Constants.Trees.Packages, null, sortOrder: 0)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadPackager))]
    public class PackagesTreeController : TreeController
    {
        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Developer, Constants.Trees.Packages, "overview");
            root.Icon = "icon-box";

            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var baseUrl = Constants.Applications.Developer + "/packages/";

            var nodes = new TreeNodeCollection();

            var createdPackages = CreatedPackage.GetAllCreatedPackages();
            
            if (id == "created")
            {
                nodes.AddRange(
                    createdPackages
                        .OrderBy(entity => entity.Data.Name)
                        .Select(dt =>
                        {
                            var node = CreateTreeNode(dt.Data.Id.ToString(), id, queryStrings, dt.Data.Name, "icon-inbox", false,
                                string.Format("/{0}/framed/{1}",
                                    queryStrings.GetValue<string>("application"),
                                    Uri.EscapeDataString("developer/Packages/EditPackage.aspx?id=" + dt.Data.Id)));                            
                            return node;
                        }));
            }
            else
            {
                //must be root
                var node = CreateTreeNode(
                    "created",
                    id,
                    queryStrings,
                    Services.TextService.Localize("treeHeaders/createdPackages"),
                    "icon-folder",
                    createdPackages.Count > 0,
                    string.Empty);

                

                //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                node.AdditionalData["jsClickCallback"] = "javascript:void(0);";

                nodes.Add(node);
            }

            

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            // Root actions              
            menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)))
                .ConvertLegacyMenuItem(null, Constants.Trees.Packages, queryStrings.GetValue<string>("application"));

            if (id == "created")
            {
                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
            }

            return menu;
        }
    }
}