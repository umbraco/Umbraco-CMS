using System.Net.Http.Formatting;
using umbraco;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Translations)]
    [Tree(Constants.Applications.Translation, "yourTasks", null, sortOrder: 2)]
    [LegacyBaseTree(typeof(loadYourTasks))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class YourTasksTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();


            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }
    }
}
