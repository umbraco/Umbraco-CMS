using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using umbraco.BusinessLogic;
using umbraco.businesslogic;

namespace Umbraco.Web.Trees
{
    //NOTE: We will of course have to authorized this but changing the base class once integrated

    /// <summary>
    /// This is used to output JSON from legacy trees
    /// </summary>
    [PluginController("UmbracoTrees")]
    public class LegacyTreeApiController : UmbracoApiController //UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Convert a legacy tree to a new tree result
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNodeCollection GetNodes(string id, FormDataCollection queryStrings)
        {
            //need to ensure we have a tree type
            var treeType = queryStrings.GetRequiredString("treeType");
            //now we'll look up that tree
            var tree = ApplicationTree.getByAlias(treeType);
            if (tree == null)
                throw new InvalidOperationException("No tree found with alias " + treeType);

            var attempt = tree.TryLoadFromLegacyTree(id, queryStrings, Url);
            if (attempt.Success == false)
            {
                throw new ApplicationException("Could not render tree " + treeType + " for node id " + id);
            }

            return attempt.Result;
        }

    }
}