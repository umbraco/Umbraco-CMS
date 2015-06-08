using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DataTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.DocumentTypes, "Document Types")]
    [Umbraco.Web.Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class DocumentTypeTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            nodes.AddRange(
                Services.ContentTypeService.GetAllContentTypes()
                    .OrderBy(x => x.Name)
                    .Select(dt => CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-item-arrangement", false)));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions              
                menu.Items.Add<CreateChildEntity, ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)));
                return menu;
            }
            else
            {
                //delete doc type
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
            }

            return menu;
        }
    }
}
