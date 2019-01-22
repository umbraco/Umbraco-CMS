using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// The content blueprint tree controller
    /// </summary>
    /// <remarks>
    /// This authorizes based on access to the content section even though it exists in the settings
    /// </remarks>
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    [Tree(Constants.Applications.Settings, Constants.Trees.ContentBlueprints, SortOrder = 12, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class ContentBlueprintTreeController : TreeController
    {

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Settings}/{Constants.Trees.ContentBlueprints}/intro";

            //check if there are any content blueprints
            root.HasChildren = Services.ContentService.GetBlueprintsForContentTypes().Any();

            return root;
        }
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            //get all blueprints
            var entities = Services.EntityService.GetChildren(Constants.System.Root, UmbracoObjectTypes.DocumentBlueprint).ToArray();

            //check if we're rendering the root in which case we'll render the content types that have blueprints
            if (id == Constants.System.Root.ToInvariantString())
            {
                //get all blueprint content types
                var contentTypeAliases = entities.Select(x => ((ContentEntitySlim) x).ContentTypeAlias).Distinct();
                //get the ids
                var contentTypeIds = Services.ContentTypeService.GetAllContentTypeIds(contentTypeAliases.ToArray()).ToArray();

                //now get the entities ... it's a bit round about but still smaller queries than getting all document types
                var docTypeEntities = contentTypeIds.Length == 0
                    ? new IUmbracoEntity[0]
                    : Services.EntityService.GetAll(UmbracoObjectTypes.DocumentType, contentTypeIds).ToArray();

                nodes.AddRange(docTypeEntities
                    .Select(entity =>
                    {
                        var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprint, id, queryStrings, "icon-item-arrangement", true);
                        treeNode.Path = $"-1,{entity.Id}";
                        treeNode.NodeType = "document-type-blueprints";
                        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                        treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return treeNode;
                    }));

                return nodes;
            }

            var intId = id.TryConvertTo<int>();
            //Get the content type
            var ct = Services.ContentTypeService.Get(intId.Result);
            if (ct == null) return nodes;

            var blueprintsForDocType = entities.Where(x => ct.Alias == ((ContentEntitySlim) x).ContentTypeAlias);
            nodes.AddRange(blueprintsForDocType
                .Select(entity =>
                {
                    var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprint, id, queryStrings, "icon-blueprint", false);
                    treeNode.Path = $"-1,{ct.Id},{entity.Id}";
                    return treeNode;
                }));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions
                menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }
            var cte = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DocumentType);
            //only refresh & create if it's a content type
            if (cte != null)
            {
                var ct = Services.ContentTypeService.Get(cte.Id);
                var createItem = menu.Items.Add<ActionCreateBlueprintFromContent>(Services.TextService, opensDialog: true);
                createItem.NavigateToRoute("/settings/contentBlueprints/edit/-1?create=true&doctype=" + ct.Alias);

                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }

            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

            return menu;
        }
    }
}
