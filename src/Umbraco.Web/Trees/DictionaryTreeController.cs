using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
	[UmbracoTreeAuthorize(Constants.Applications.Settings, Constants.Trees.Dictionary)]
	[LegacyBaseTree(typeof(loadDictionary))]
	[Tree(Constants.Applications.Settings, Constants.Trees.Dictionary, "Dictionary")]
	[PluginController("UmbracoTrees")]
	[CoreTree]
	public class DictionaryTreeController : TreeController
	{
		protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
		{
			var root = base.CreateRootNode(queryStrings);

			root.RoutePath = string.Join("/", root.RoutePath, Constants.Trees.Dictionary, "list");

			return root;
		}

		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
		{
			var collection = new TreeNodeCollection();

			collection.AddRange(
				GetDictionaryItems(id)
				.OrderBy(x => x.Id)
				.Select(x => CreateTreeNode(id, queryStrings, x)));

			return collection;
		}

		private IEnumerable<IDictionaryItem> GetDictionaryItems(string id)
		{
			int dictionaryId;
			if (int.TryParse(id, out dictionaryId) && dictionaryId > Constants.System.Root)
			{
				var dictionaryItem = Services.LocalizationService.GetDictionaryItemById(dictionaryId);

				if (dictionaryItem != null)
					return Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key);
			}

			return Services.LocalizationService.GetRootDictionaryItems();
		}

		private TreeNode CreateTreeNode(string id, FormDataCollection queryStrings, IDictionaryItem dictionaryItem)
		{
			var hasChildren = Services.LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key).Any();

			var node = CreateTreeNode(
				dictionaryItem.Id.ToInvariantString(),
				id,
				queryStrings,
				dictionaryItem.ItemKey,
				"icon-book-alt",
				hasChildren);

			return node;
		}

		protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			var menu = new MenuItemCollection();

			menu.Items.Add<CreateChildEntity, ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));

			if (id != Constants.System.Root.ToInvariantString())
			{
				menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
			}

			menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

			return menu;
		}
	}
}