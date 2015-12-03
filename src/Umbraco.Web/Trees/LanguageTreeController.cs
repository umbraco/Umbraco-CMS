using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Web.Services.Description;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.presentation.actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Languages)]
    [LegacyBaseTree(typeof(loadLanguages))]
    [Tree(Constants.Applications.Settings, Constants.Trees.Languages, null, sortOrder: 4)]
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
                            Uri.EscapeDataString("settings/editLanguage.aspx?id=" + language.Id)));
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
                menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias))
                    //Since we haven't implemented anything for languages in angular, this needs to be converted to 
                    //use the legacy format
                    .ConvertLegacyMenuItem(null, "initlanguages", queryStrings.GetValue<string>("application"));
               
                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);

                return menu;
            }

            var lang = Services.LocalizationService.GetLanguageById(int.Parse(id));
            if (lang == null) return new MenuItemCollection();

            //add delete option for all languages
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias))
                //Since we haven't implemented anything for languages in angular, this needs to be converted to 
                //use the legacy format
                .ConvertLegacyMenuItem(new UmbracoEntity
                {
                    Id = lang.Id,
                    Level = 1,
                    ParentId = -1,
                    Name = lang.CultureInfo.DisplayName                    
                }, "language", queryStrings.GetValue<string>("application"));

            return menu;
        }
    }
}
