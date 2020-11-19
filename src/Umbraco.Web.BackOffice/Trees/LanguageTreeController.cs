using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Languages, SortOrder = 11, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class LanguageTreeController : TreeController
    {

        public LanguageTreeController(
            ILocalizedTextService textService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection)
            : base(textService, umbracoApiControllerTypeCollection)
        {
        }
        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
        {
            //We don't have any child nodes & only use the root node to load a custom UI
            return new TreeNodeCollection();
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
        {
            //We don't have any menu item options (such as create/delete/reload) & only use the root node to load a custom UI
            return null;
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Settings}/{Constants.Trees.Languages}/overview";
            root.Icon = "icon-globe";
            root.HasChildren = false;
            root.MenuUrl = null;

            return root;
        }


    }
}
