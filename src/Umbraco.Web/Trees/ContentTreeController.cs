using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content, 
        Constants.Applications.Media, 
        Constants.Applications.Users,
        Constants.Applications.Settings,
        Constants.Applications.Developer,
        Constants.Applications.Members)]
    [LegacyBaseTree(typeof(loadContent))]
    [Tree(Constants.Applications.Content, Constants.Trees.Content, "Content")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
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
            node.Name = ui.Text("sections", Constants.Trees.Content);
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
        
        /// <summary>
        /// Creates a tree node for a content item based on an UmbracoEntity
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode GetSingleTreeNode(IUmbracoEntity e, string parentId, FormDataCollection queryStrings)
        {
            var entity = (UmbracoEntity) e;

            var allowedUserOptions = GetAllowedUserMenuItemsForNode(e);
            if (CanUserAccessNode(e, allowedUserOptions))
            {

                //Special check to see if it ia a container, if so then we'll hide children.
                var isContainer = e.IsContainer();   // && (queryStrings.Get("isDialog") != "true");

                var node = CreateTreeNode(
                    e.Id.ToInvariantString(),
                    parentId,
                    queryStrings,
                    e.Name,
                    entity.ContentTypeIcon,
                    entity.HasChildren && (isContainer == false));

                node.AdditionalData.Add("contentType", entity.ContentTypeAlias);

                if (isContainer)
                {
                    node.AdditionalData.Add("isContainer", true);
                    node.SetContainerStyle();
                }
                    

                if (entity.IsPublished == false)
                    node.SetNotPublishedStyle();

                if (entity.HasPendingChanges)
                    node.SetHasUnpublishedVersionStyle();

                if (Services.PublicAccessService.IsProtected(e.Path))
                    node.SetProtectedStyle();

                return node;
            }

            return null;
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
                menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(null, "content", "content");

                //filter the standard items
                FilterUserAllowedMenuItems(menu, nodeActions);

                if (menu.Items.Any())
                {
                    menu.Items.Last().SeperatorBefore = true;
                }

                // add default actions for *all* users
                menu.Items.Add<ActionRePublish>(ui.Text("actions", ActionRePublish.Instance.Alias)).ConvertLegacyMenuItem(null, "content", "content");
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                
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

            //if the media item is in the recycle bin, don't have a default menu, just show the regular menu
            if (item.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                nodeMenu.DefaultMenuAlias = null;
                nodeMenu.Items.Insert(2, new MenuItem(ActionRestore.Instance, ui.Text("actions", ActionRestore.Instance.Alias)));
            }
            else
            {
                //set the default to create
                nodeMenu.DefaultMenuAlias = ActionNew.Instance.Alias;    
            }
            

            return nodeMenu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Document; }
        }

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override bool HasPathAccess(string id, FormDataCollection queryStrings)
        {
            var content = Services.ContentService.GetById(int.Parse(id));
            if (content == null)
            {
                return false;
            }
            return Security.CurrentUser.HasPathAccess(content);
        }

        /// <summary>
        /// Returns a collection of all menu items that can be on a content node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected MenuItemCollection GetAllNodeMenuItems(IUmbracoEntity item)
        {
            var menu = new MenuItemCollection();
            menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
            
            //need to ensure some of these are converted to the legacy system - until we upgrade them all to be angularized.
            menu.Items.Add<ActionMove>(ui.Text("actions", ActionMove.Instance.Alias), true);
            menu.Items.Add<ActionCopy>(ui.Text("actions", ActionCopy.Instance.Alias));
            menu.Items.Add<ActionChangeDocType>(ui.Text("actions", ActionChangeDocType.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<ActionRollback>(ui.Text("actions", ActionRollback.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionAudit>(ui.Text("actions", ActionAudit.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionPublish>(ui.Text("actions", ActionPublish.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionToPublish>(ui.Text("actions", ActionToPublish.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionAssignDomain>(ui.Text("actions", ActionAssignDomain.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionRights>(ui.Text("actions", ActionRights.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionProtect>(ui.Text("actions", ActionProtect.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            
            menu.Items.Add<ActionNotify>(ui.Text("actions", ActionNotify.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionSendToTranslate>(ui.Text("actions", ActionSendToTranslate.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

            return menu;
        }

    }
}