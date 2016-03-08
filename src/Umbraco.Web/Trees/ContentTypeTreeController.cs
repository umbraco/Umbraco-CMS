using System;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.DocumentTypes, null, sortOrder: 6)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    [LegacyBaseTree(typeof(loadNodeTypes))]
    public class ContentTypeTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false) throw new InvalidOperationException("Id must be an integer");

            var nodes = new TreeNodeCollection();

            nodes.AddRange(
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DocumentTypeContainer)
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
                Services.EntityService.GetChildren(intId.Result, UmbracoObjectTypes.DocumentType)
                    .OrderBy(entity => entity.Name)
                    .Select(dt =>
                    {
                        var node = CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, "icon-item-arrangement", 
                            //NOTE: This is legacy now but we need to support upgrades. From 7.4+ we don't allow 'child' creations since
                            // this is an organiational thing and we do that with folders now.
                            dt.HasChildren());

                        node.Path = dt.Path;
                        return node;
                    }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            var enableInheritedDocumentTypes = UmbracoConfig.For.UmbracoSettings().Content.EnableInheritedDocumentTypes;

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions              
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                menu.Items.Add<ActionImport>(Services.TextService.Localize(string.Format("actions/{0}", ActionImport.Instance.Alias)), true).ConvertLegacyMenuItem(new UmbracoEntity
                {
                    Id = int.Parse(id),
                    Level = 1,
                    ParentId = Constants.System.Root,
                    Name = ""
                }, "documenttypes", "settings");
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
                return menu;
            }

            var container = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DocumentTypeContainer);
            if (container != null)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));

                if (container.HasChildren() == false)
                {
                    //can delete doc type
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)), true);
                }
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);    
            }
            else
            {
                var ct = Services.ContentTypeService.GetContentType(int.Parse(id));
                IContentType parent = null;
                parent = ct == null ? null : Services.ContentTypeService.GetContentType(ct.ParentId);

                if (enableInheritedDocumentTypes)
                {
                    menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));

                    //no move action if this is a child doc type
                    if (parent == null)
                    {
                        menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)), true);
                    }
                }
                else
                {
                    menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)));
                    //no move action if this is a child doc type
                    if (parent == null)
                    {
                        menu.Items.Add<ActionMove>(Services.TextService.Localize(string.Format("actions/{0}", ActionMove.Instance.Alias)), true);
                    }
                }
                menu.Items.Add<ActionCopy>(Services.TextService.Localize(string.Format("actions/{0}", ActionCopy.Instance.Alias)));
                menu.Items.Add<ActionExport>(Services.TextService.Localize(string.Format("actions/{0}", ActionExport.Instance.Alias)), true).ConvertLegacyMenuItem(new UmbracoEntity
                {
                    Id = int.Parse(id),
                    Level = 1,
                    ParentId = Constants.System.Root,
                    Name = ""
                }, "documenttypes", "settings");
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)), true);
                if (enableInheritedDocumentTypes)
                    menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
            }

            return menu;
        }
    }
}
