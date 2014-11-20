using System;
using System.Globalization;
using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Languages)]
    [LegacyBaseTree(typeof(loadLanguages))]
    [Tree(Constants.Applications.Settings, Constants.Trees.Languages, "Languages")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class LanguageTreeController : TreeController
    {
        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                var languages = Services.LocalizationService.GetAllLanguages();
                foreach (var language in languages)
                {
                    nodes.Add(
                        CreateTreeNode(
                            language.Id.ToString(CultureInfo.InvariantCulture), "-1", queryStrings, language.CultureInfo.DisplayName, "icon-flag-alt", false,
                            //TODO: Rebuild the language editor in angular, then we dont need to have this at all (which is just a path to the legacy editor)
                            "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                            Uri.EscapeDataString("/umbraco/settings/editLanguage.aspx?id=" + language.Id)));
                }
            }
            return nodes;
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create the normal create action
                var createMenuItem = menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                createMenuItem.LaunchDialogUrl("/umbraco/create.aspx?nodeId=init&nodeType=initlanguages&nodeName=Languages", ui.GetText("general", "create"));

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                return menu;
            }

            //add delete option for all languages
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));

            return menu;
        }
    }
}