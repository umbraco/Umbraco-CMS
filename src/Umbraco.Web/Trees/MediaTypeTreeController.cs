using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.MediaTypes, null, sortOrder:5)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadMediaTypes))]
    public class MediaTypeTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaTypeContainer)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-folder", dt.HasChildren(), "");
                        node.Path = dt.Path;
                        node.NodeType = "container";
                        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                        node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return node;
                    }));

            //if the request is for folders only then just return
            if (queryStrings["foldersonly"].IsNullOrWhiteSpace() == false && queryStrings["foldersonly"] == "1") return nodes;

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.MediaType)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-item-arrangement", false);
                        node.Path = dt.Path;
                        return node;
                    }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions              
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)));
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.MediaTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                
                if (container.HasChildren() == false)
                {
                    //can delete doc type
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
                }                
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), hasSeparator: true);
            }
            else
            {                
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
                menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)), hasSeparator: true);
                menu.Items.Add<ActionCopy>(Services.TextService.Localize(string.Format("actions/{0}", ActionCopy.Instance.Alias)));
            }

            return menu;
        }
    }
}
