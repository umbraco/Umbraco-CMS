using System;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [LegacyBaseTree(typeof(loadMedia))]
    [Tree(Constants.Applications.Media, Constants.Trees.Media, "Media")]
    [PluginController("UmbracoTrees")]
    public class MediaTreeController : ContentTreeControllerBase
    {
        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinContent; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.MediaService.RecycleBinSmells(); }
        }

        protected override TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var entities = GetChildEntities(id);

            var nodes = new TreeNodeCollection();            

            nodes.AddRange(
                entities.Cast<UmbracoEntity>()
                        .Select(e => CreateTreeNode(e.Id.ToInvariantString(), queryStrings, e.Name, e.ContentTypeIcon, e.HasChildren)));

            return nodes;

        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions         
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>(true);
                menu.AddMenuItem<RefreshNode, ActionRefresh>(true);
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
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionMove>().ConvertLegacyMenuItem(item, "media", "media");
            menu.AddMenuItem<ActionDelete>();
            menu.AddMenuItem<ActionSort>();
            menu.AddMenuItem<ActionRefresh>(true);
            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Media; }
        }
    }
}