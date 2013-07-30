using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Content, Constants.Trees.Content, "Content")]
    public class ContentTreeController : ContentTreeControllerBase
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            //TODO: We need to implement security checks here and the user's start node!

            return base.CreateRootNode(queryStrings);
        }

        protected override TreeNodeCollection GetTreeData(string id, FormDataCollection queryStrings)
        {
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new InvalidCastException("The id for the content tree must be an integer");
            }

            var nodes = new TreeNodeCollection();
            var entities = Services.EntityService.GetChildren(iid, UmbracoObjectTypes.Document).ToArray();
            foreach (var entity in entities)
            {
                //TODO: We need to implement security checks here!

                var e = (UmbracoEntity)entity;
                nodes.Add(
                    CreateTreeNode(
                        e.Id.ToInvariantString(),
                        queryStrings,
                        e.Name,
                        e.ContentTypeIcon,
                        e.HasChildren));
            }
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
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
                GetUserMenuItemsForNode((UmbracoEntity) item));
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