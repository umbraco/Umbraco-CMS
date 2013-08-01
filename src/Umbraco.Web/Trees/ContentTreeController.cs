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
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    //[Tree(Constants.Applications.Content, Constants.Trees.Content, "Content")]
    //public class MediaTreeController : ContentTreeControllerBase
    //{
    //    protected override TreeNodeCollection GetTreeData(string id, FormDataCollection queryStrings)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    [Tree(Constants.Applications.Content, Constants.Trees.Content, "Content")]
    public class ContentTreeController : ContentTreeControllerBase
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            //if the user's start node is not default, then return their start node as the root node.
            if (UmbracoUser.StartNodeId != Constants.System.Root)
            {
                var currApp = queryStrings.GetValue<string>(TreeQueryStringParameters.Application);
                var userRoot = Services.EntityService.Get(UmbracoUser.StartNodeId, UmbracoObjectTypes.Document);
                if (userRoot == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                var node = new TreeNode(
                    userRoot.Id.ToInvariantString(),
                    "", //root nodes aren't expandable, no need to lookup the child nodes url
                    Url.GetMenuUrl(GetType(), userRoot.Id.ToInvariantString(), queryStrings))
                {
                    HasChildren = true,
                    RoutePath = currApp,
                    Title = userRoot.Name
                };

                return node;
            }

            return base.CreateRootNode(queryStrings);
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
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new InvalidCastException("The id for the content tree must be an integer");
            }

            var nodes = new TreeNodeCollection();
            IEnumerable<IUmbracoEntity> entities;

            //if a request is made for the root node data but the user's start node is not the default, then
            // we need to return their start node data
            if (iid == Constants.System.Root && UmbracoUser.StartNodeId != Constants.System.Root)
            {
                entities = Services.EntityService.GetChildren(UmbracoUser.StartNodeId, UmbracoObjectTypes.Document).ToArray();
            }
            else
            {
                entities = Services.EntityService.GetChildren(iid, UmbracoObjectTypes.Document).ToArray();                
            }

            foreach (var entity in entities)
            {
                var e = (UmbracoEntity)entity;
                
                var allowedUserOptions = GetUserMenuItemsForNode(e);
                if (CanUserAccessNode(e, allowedUserOptions))
                {
                    nodes.Add(
                    CreateTreeNode(
                        e.Id.ToInvariantString(),
                        queryStrings,
                        e.Name,
                        e.ContentTypeIcon,
                        e.HasChildren));
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

                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>();
                var allowedMenu = GetUserAllowedMenuItems(menu, nodeActions);

                if (allowedMenu.Any())
                {
                    allowedMenu.Last().SeperatorBefore = true;
                }

                // default actions for all users
                allowedMenu.AddMenuItem<ActionRePublish>();
                allowedMenu.AddMenuItem<ActionRefresh>(true);
                return allowedMenu;
            }

            //return a normal node menu:
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new InvalidOperationException("The Id for a content item must be an integer");
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Document);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return GetUserAllowedMenuItems(
                CreateAllowedActions(), 
                GetUserMenuItemsForNode(item));
        }

        protected IEnumerable<MenuItem> CreateAllowedActions()
        {
            var menu = new MenuItemCollection();
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionDelete>(true);
            menu.AddMenuItem<ActionMove>(true);
            menu.AddMenuItem<ActionCopy>();
            menu.AddMenuItem<ActionSort>(true);
            menu.AddMenuItem<ActionRollback>();
            menu.AddMenuItem<ActionPublish>(true);
            menu.AddMenuItem<ActionToPublish>();
            menu.AddMenuItem<ActionAssignDomain>();
            menu.AddMenuItem<ActionRights>();
            menu.AddMenuItem<ActionProtect>(true);
            menu.AddMenuItem<ActionUnPublish>(true);
            menu.AddMenuItem<ActionNotify>(true);
            menu.AddMenuItem<ActionSendToTranslate>();
            menu.AddMenuItem<ActionRefresh>(true);
            return menu;
        }

        
    }
}