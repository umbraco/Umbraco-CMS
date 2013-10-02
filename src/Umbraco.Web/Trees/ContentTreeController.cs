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
            var node = base.CreateRootNode(queryStrings); 
            //if the user's start node is not default, then ensure the root doesn't have a menu
            if (Security.CurrentUser.StartContentId != Constants.System.Root)
            {
                node.MenuUrl = "";
            }
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

        protected override int UserStartNode
        {
            get { return Security.CurrentUser.StartContentId; }
        }

        protected override TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var entities = GetChildEntities(id);

            var nodes = new TreeNodeCollection();
            
            foreach (var entity in entities)
            {
                var e = (UmbracoEntity)entity;
               
                var allowedUserOptions = GetAllowedUserMenuItemsForNode(e);
                if (CanUserAccessNode(e, allowedUserOptions))
                {                    
                    var hasChildren = e.HasChildren;

                    //Special check to see if it ia a container, if so then we'll hide children.
                    if (entity.AdditionalData["IsContainer"] is bool && (bool) entity.AdditionalData["IsContainer"])
                    {
                        hasChildren = false;
                    }

                    var node = CreateTreeNode(
                        e.Id.ToInvariantString(),
                        queryStrings,
                        e.Name,
                        e.ContentTypeIcon,
                        hasChildren);

                    nodes.Add(node);
                }
            }
            return nodes;
        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.Root.ToInvariantString())
            {
                var menu = new MenuItemCollection();

                //if the user's start node is not the root then ensure the root menu is empty/doesn't exist
                if (Security.CurrentUser.StartContentId != Constants.System.Root)
                {
                    return menu;
                }

                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // we need to get the default permissions as you can't set permissions on the very root node
                //TODO: Use the new services to get permissions
                var nodeActions = global::umbraco.BusinessLogic.Actions.Action.FromString(
                    UmbracoUser.GetPermissions(Constants.System.Root.ToInvariantString()))
                                        .Select(x => new MenuItem(x));

                //these two are the standard items
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>(true).ConvertLegacyMenuItem(null, "content", "content");

                //filter the standard items
                FilterUserAllowedMenuItems(menu, nodeActions);

                if (menu.MenuItems.Any())
                {
                    menu.MenuItems.Last().SeperatorBefore = true;
                }

                // add default actions for *all* users
                menu.AddMenuItem<ActionRePublish>().ConvertLegacyMenuItem(null, "content", "content");
                menu.AddMenuItem<RefreshNode, ActionRefresh>(true);

                foreach (var menuItem in menu.MenuItems)
                {
                    menuItem.Name = ui.Text("actions", menuItem.Alias);
                }

                return menu;
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

            var nodeMenu = GetAllNodeMenuItems(item);
            var allowedMenuItems = GetAllowedUserMenuItemsForNode(item);
                
            FilterUserAllowedMenuItems(nodeMenu, allowedMenuItems);

            //set the default to create
            nodeMenu.DefaultMenuAlias = ActionNew.Instance.Alias;

            foreach (var menuItem in nodeMenu.MenuItems)
            {
                menuItem.Name = ui.Text("actions", menuItem.Alias);
            }
            return nodeMenu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Document; }
        }

        /// <summary>
        /// Returns a collection of all menu items that can be on a content node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected MenuItemCollection GetAllNodeMenuItems(IUmbracoEntity item)
        {
            var menu = new MenuItemCollection();
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionDelete>(true);
            
            //need to ensure some of these are converted to the legacy system - until we upgrade them all to be angularized.
            menu.AddMenuItem<ActionMove>(true);
            menu.AddMenuItem<ActionCopy>();
            
            menu.AddMenuItem<ActionSort>(true).ConvertLegacyMenuItem(item, "content", "content");

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

    }
}