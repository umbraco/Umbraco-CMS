using System;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [LegacyBaseTree(typeof(loadMedia))]
    [Tree(Constants.Applications.Media, Constants.Trees.Media, "Media")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class MediaTreeController : ContentTreeControllerBase
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var node = base.CreateRootNode(queryStrings);
            //if the user's start node is not default, then ensure the root doesn't have a menu
            if (Security.CurrentUser.StartMediaId != Constants.System.Root)
            {
                node.MenuUrl = "";
            }
            return node;
        }

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.MediaService.RecycleBinSmells(); }
        }

        protected override int UserStartNode
        {
            get { return Security.CurrentUser.StartMediaId; }
        }

        protected override TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var entities = GetChildEntities(id);
            var nodes = new TreeNodeCollection();

            foreach (var entity in entities)
            {
                var e = (UmbracoEntity)entity;
                var node = CreateTreeNode(e.Id.ToInvariantString(), id, queryStrings, e.Name, e.ContentTypeIcon, e.HasChildren);
                
                node.AdditionalData.Add("contentType", e.ContentTypeAlias);
                nodes.Add(node);
            }
            return nodes;

        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            if (id == Constants.System.Root.ToInvariantString())
            {
                //if the user's start node is not the root then ensure the root menu is empty/doesn't exist
                if (Security.CurrentUser.StartMediaId != Constants.System.Root)
                {
                    return menu;
                }

                // root actions         
                menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias), true).ConvertLegacyMenuItem(null, "media", "media");
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Media);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //return a normal node menu:
            menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
            menu.Items.Add<ActionMove>(ui.Text("actions", ActionMove.Instance.Alias));
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
            menu.Items.Add<ActionSort>(ui.Text("actions", ActionSort.Instance.Alias)).ConvertLegacyMenuItem(item, "media", "media");
            menu.Items.Add<ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

            //if the media item is in the recycle bin, don't have a default menu, just show the regular menu
            if (item.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Contains(RecycleBinId.ToInvariantString()))
            {
                menu.DefaultMenuAlias = null;
            }

            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Media; }
        }
    }
}