using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Dictionary)]
    [Tree(Constants.Applications.Translation, Constants.Trees.Dictionary, null, sortOrder: 3)]
    [Mvc.PluginController("UmbracoTrees")]
    [CoreTree]
    public class TranslationDictionaryTreeController : DictionaryTreeBaseController
    {
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            // this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Translation}/{Constants.Trees.Dictionary}/list";

            return root;
        }
    }
}
