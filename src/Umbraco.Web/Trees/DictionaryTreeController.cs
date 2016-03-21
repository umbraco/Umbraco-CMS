using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Dictionary, null, sortOrder: 3)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class DictionaryTreeController : TreeController
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var node = base.CreateRootNode(queryStrings);
            
            // For now, this is using the legacy webforms view but will need refactoring
            // when the dictionary has been converted to Angular.
            node.RoutePath = String.Format("{0}/framed/{1}", queryStrings.GetValue<string>("application"),
                                           Uri.EscapeDataString("settings/DictionaryItemList.aspx"));

            return node;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var intId = ValidateId(id);

            var nodes = new TreeNodeCollection();
            nodes.AddRange(GetDictionaryItems(intId)
                               .OrderBy(dictionaryItem => dictionaryItem.ItemKey)
                               .Select(dictionaryItem => CreateTreeNode(id, queryStrings, dictionaryItem)));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var intId = ValidateId(id);

            var menu = new MenuItemCollection();

            if (intId == Constants.System.Root)
            {
                // Again, menu actions will need to use legacy views as this section hasn't been converted to Angular (yet!)
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    .ConvertLegacyMenuItem(null, "dictionary", queryStrings.GetValue<string>("application"));

                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
            }
            else
            {
                
                var dictionaryItem = Services.LocalizationService.GetDictionaryItemById(intId);
                var entity = new UmbracoEntity
                    {
                        Id = dictionaryItem.Id,
                        Level = 1,
                        ParentId = -1,
                        Name = dictionaryItem.ItemKey
                    };

                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    .ConvertLegacyMenuItem(entity, "dictionary", queryStrings.GetValue<string>("application"));

                menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias))
                    .ConvertLegacyMenuItem(null, "dictionary", queryStrings.GetValue<string>("application"));

                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
            }

            return menu;
        }

        private IEnumerable<IDictionaryItem> GetDictionaryItems(int id)
        {
            if (id > Constants.System.Root)
            {
                var dictionaryItem = Services.LocalizationService.GetDictionaryItemById(id);

                if (dictionaryItem != null)
                {
                    return Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key);
                }
            }

            return Services.LocalizationService.GetRootDictionaryItems();
        }

        private TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, IDictionaryItem dictionaryItem)
        {
            var hasChildren = Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key).Any();

            // Again, menu actions will need to use legacy views as this section hasn't been converted to Angular (yet!)
            var node = CreateTreeNode(dictionaryItem.Id.ToInvariantString(), id, queryStrings, dictionaryItem.ItemKey,
                                      "icon-book-alt", hasChildren,
                                      String.Format("{0}/framed/{1}", queryStrings.GetValue<string>("application"),
                                                    Uri.EscapeDataString("settings/editDictionaryItem.aspx?id=" +
                                                                         dictionaryItem.Id)));

            return node;
        }

        private int ValidateId(string id)
        {
            var intId = id.TryConvertTo<int>();
            if (intId == false)
            {
                throw new InvalidOperationException("Id must be an integer");
            }

            return intId.Result;
        }
    }
}
