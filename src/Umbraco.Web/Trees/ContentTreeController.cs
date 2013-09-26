using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.interfaces;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [LegacyBaseTree(typeof(loadContent))]
    [Tree(Constants.Applications.Content, Constants.Trees.Content, "Content")]
    [PluginController("UmbracoTrees")]
    public class ContentTreeController : ContentTreeControllerBase
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            TreeNode node;

            //if the user's start node is not default, then return their start node as the root node.
            if (UmbracoUser.StartNodeId != Constants.System.Root)
            {
                var currApp = queryStrings.GetValue<string>(TreeQueryStringParameters.Application);
                var userRoot = Services.EntityService.Get(UmbracoUser.StartNodeId, UmbracoObjectTypes.Document);
                if (userRoot == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                node = new TreeNode(
                    userRoot.Id.ToInvariantString(),
                    "", //root nodes aren't expandable, no need to lookup the child nodes url
                    Url.GetMenuUrl(GetType(), userRoot.Id.ToInvariantString(), queryStrings))
                    {
                        HasChildren = true,
                        RoutePath = currApp,
                        Title = userRoot.Name
                    };


            }
            else
            {
                node = base.CreateRootNode(queryStrings);    
            }

            //InjectLegacyTreeCompatibility(node, queryStrings);

            return node;
            
        }

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinContent; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.ContentService.RecycleBinSmells(); }
        }

        protected override TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var entities = GetChildEntities(id);

            var nodes = new TreeNodeCollection();
            
            foreach (var entity in entities)
            {
                var e = (UmbracoEntity)entity;
               
                var allowedUserOptions = GetUserMenuItemsForNode(e);
                if (CanUserAccessNode(e, allowedUserOptions))
                {
                    //TODO: if the node is of a specific type, don't list its children
                    //this is to enable the list view on the editor

                    //for WIP I'm just checking against a hardcoded value
                    var hasChildren = e.HasChildren;
                    if (e.ContentTypeAlias == "umbNewsArea")
                        hasChildren = false;

                    var node = CreateTreeNode(
                        e.Id.ToInvariantString(),
                        queryStrings,
                        e.Name,
                        e.ContentTypeIcon,
                        hasChildren);

                    //InjectLegacyTreeCompatibility(node, queryStrings);

                    nodes.Add(node);
                }
            }
            return nodes;
        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // we need to get the default permissions as you can't set permissions on the very root node
                var nodeActions = global::umbraco.BusinessLogic.Actions.Action.FromString(
                    UmbracoUser.GetPermissions(Constants.System.Root.ToInvariantString()))
                                        .Select(x => new MenuItem(x));

                //these two are the standard items
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>();

                //filter the standard items
                var allowedMenu = GetUserAllowedMenuItems(menu, nodeActions);

                if (allowedMenu.Any())
                {
                    allowedMenu.Last().SeperatorBefore = true;
                }

                // add default actions for *all* users
                allowedMenu.AddMenuItem<ActionRePublish>().ConvertLegacyMenuItem(null, "content", "content");
                allowedMenu.AddMenuItem<RefreshNode, ActionRefresh>(true);
                return allowedMenu;
            }

            //return a normal node menu:
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Document);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return GetUserAllowedMenuItems(
                CreateAllowedActions(item), 
                GetUserMenuItemsForNode(item));
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Document; }
        }

        protected IEnumerable<MenuItem> CreateAllowedActions(IUmbracoEntity item)
        {
            var menu = new MenuItemCollection();
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionDelete>(true);
            
            //need to ensure some of these are converted to the legacy system - until we upgrade them all to be angularized.
            menu.AddMenuItem<ActionMove>(true).ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionCopy>().ConvertLegacyMenuItem(item, "content", "content");
            
            menu.AddMenuItem<ActionSort>(true);

            menu.AddMenuItem<ActionRollback>().ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionPublish>(true).ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionToPublish>().ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionAssignDomain>().ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionRights>().ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionProtect>(true).ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionUnPublish>(true).ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionNotify>(true).ConvertLegacyMenuItem(item, "content", "content");
            menu.AddMenuItem<ActionSendToTranslate>().ConvertLegacyMenuItem(item, "content", "content");

            menu.AddMenuItem<RefreshNode, ActionRefresh>(true);

            return menu;
        }

        ///// <summary>
        ///// This is required so that the legacy tree dialog pickers and the legacy TreeControl.ascx stuff works with these new trees.
        ///// </summary>
        ///// <param name="node"></param>
        ///// <param name="queryStrings"></param>
        ///// <remarks>
        ///// NOTE: That if developers create brand new trees using webapi controllers then they'll need to manually make it 
        ///// compatible with the legacy tree pickers, 99.9% of the time devs won't be doing that and once we make the new tree
        ///// pickers and devs update their apps to be angularized it won't matter
        ///// </remarks>
        //private void InjectLegacyTreeCompatibility(TreeNode node, FormDataCollection queryStrings)
        //{
        //    if (IsDialog(queryStrings))
        //    {
        //        node.AdditionalData["legacyDialogAction"] = "javascript:openContent(" + node.NodeId + ");";
        //    }
        //}
        
    }
}