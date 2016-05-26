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
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes)]
    [Tree(Constants.Applications.Developer, "packagesNew")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class NewPackagesTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var node = CreateTreeNode("1", id, queryStrings, "Name", "icon-folder", false, "");
            node.Path = "path";
            //node.NodeType = "container";
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            //node.AdditionalData["jsClickCallback"] = "javascript:void(0);";

            nodes.Add(node);

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            return menu;
        }
    }
}