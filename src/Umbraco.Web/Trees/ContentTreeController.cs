﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;
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
    [Tree(Constants.Applications.Content, Constants.Trees.Content)]
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
            node.Name = Services.TextService.Localize("sections/"+ Constants.Trees.Content);
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
                var permission = Services.UserService.GetPermissions(Security.CurrentUser, Constants.System.Root).First();
                var nodeActions = global::Umbraco.Web._Legacy.Actions.Action.FromEntityPermission(permission)
                    .Select(x => new MenuItem(x));

                //these two are the standard items
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionSort>(Services.TextService.Localize("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(null, "content", "content");

                //filter the standard items
                FilterUserAllowedMenuItems(menu, nodeActions);

                if (menu.Items.Any())
                {
                    menu.Items.Last().SeperatorBefore = true;
                }

                // add default actions for *all* users
                menu.Items.Add<ActionRePublish>(Services.TextService.Localize("actions", ActionRePublish.Instance.Alias)).ConvertLegacyMenuItem(null, "content", "content");
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
                
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
                nodeMenu.Items.Insert(2, new MenuItem(ActionRestore.Instance, Services.TextService.Localize("actions", ActionRestore.Instance.Alias)));
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
            menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias));
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias));
            
            //need to ensure some of these are converted to the legacy system - until we upgrade them all to be angularized.
            menu.Items.Add<ActionMove>(Services.TextService.Localize("actions", ActionMove.Instance.Alias), true);
            menu.Items.Add<ActionCopy>(Services.TextService.Localize("actions", ActionCopy.Instance.Alias));
            menu.Items.Add<ActionChangeDocType>(Services.TextService.Localize("actions", ActionChangeDocType.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<ActionSort>(Services.TextService.Localize("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<ActionRollback>(Services.TextService.Localize("actions", ActionRollback.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionAudit>(Services.TextService.Localize("actions", ActionAudit.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionPublish>(Services.TextService.Localize("actions", ActionPublish.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionToPublish>(Services.TextService.Localize("actions", ActionToPublish.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionAssignDomain>(Services.TextService.Localize("actions", ActionAssignDomain.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionRights>(Services.TextService.Localize("actions", ActionRights.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionProtect>(Services.TextService.Localize("actions", ActionProtect.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            
            menu.Items.Add<ActionNotify>(Services.TextService.Localize("actions", ActionNotify.Instance.Alias), true).ConvertLegacyMenuItem(item, "content", "content");
            menu.Items.Add<ActionSendToTranslate>(Services.TextService.Localize("actions", ActionSendToTranslate.Instance.Alias)).ConvertLegacyMenuItem(item, "content", "content");

            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

            return menu;
        }

    }
}