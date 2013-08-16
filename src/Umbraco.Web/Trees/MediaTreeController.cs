using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
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

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions         
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>(true);
                menu.AddMenuItem<RefreshNodeMenuItem, ActionRefresh>(true);
                return menu;
            }

            //return a normal node menu:
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionMove>();
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