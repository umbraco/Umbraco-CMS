using System;
using System.Globalization;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

using umbraco;

namespace Umbraco.Web.Trees
{
    [AngularJsonOnlyConfiguration]
    [PluginController("UmbracoTrees")]
    public class ApplicationTreeController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Returns the tree nodes for an application
        /// </summary>
        /// <param name="application">The application to load tree for</param>
        /// <param name="tree">An optional single tree alias, if specified will only load the single tree for the request app</param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public async Task<SectionRootNode> GetApplicationTrees(string application, string tree, FormDataCollection queryStrings)
        {
            if (string.IsNullOrEmpty(application)) throw new HttpResponseException(HttpStatusCode.NotFound);

            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);

            //find all tree definitions that have the current application alias
            var appTrees = ApplicationContext.Current.Services.ApplicationTreeService.GetApplicationTrees(application, true).ToArray();

            if (appTrees.Count() == 1 || string.IsNullOrEmpty(tree) == false )
            {
                var apptree = string.IsNullOrEmpty(tree) == false 
                    ? appTrees.SingleOrDefault(x => x.Alias == tree)
                    : appTrees.SingleOrDefault();

                if (apptree == null) throw new HttpResponseException(HttpStatusCode.NotFound);

                var result = await GetRootForSingleAppTree(
                    apptree,
                    Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                    queryStrings, 
                    application);

                return result;
            }


            var collection = new TreeNodeCollection();
            foreach (var apptree in appTrees)
            {
                //return the root nodes for each tree in the app
                var rootNode = await GetRootForMultipleAppTree(apptree, queryStrings);
                //This could be null if the tree decides not to return it's root (i.e. the member type tree does this when not in umbraco membership mode)
                if (rootNode != null)
                {
                    collection.Add(rootNode);     
                }
            }

            var multiTree = SectionRootNode.CreateMultiTreeSectionRoot(rootId, collection);
            multiTree.Name = ui.Text("sections", application);
            return multiTree;
        }

        /// <summary>
        /// Get the root node for an application with multiple trees
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private async Task<TreeNode> GetRootForMultipleAppTree(ApplicationTree configTree, FormDataCollection queryStrings)
        {
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = await configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext);
            if (byControllerAttempt.Success)
            {
                return byControllerAttempt.Result;
            }

            var legacyAttempt = configTree.TryGetRootNodeFromLegacyTree(queryStrings, Url, configTree.ApplicationAlias);
            if (legacyAttempt.Success)
            {
                return legacyAttempt.Result;
            }

            throw new ApplicationException("Could not get root node for tree type " + configTree.Alias);
        }

        /// <summary>
        /// Get the root node for an application with one tree
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private async Task<SectionRootNode> GetRootForSingleAppTree(ApplicationTree configTree, string id, FormDataCollection queryStrings, string application)
        {
            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = configTree.TryLoadFromControllerTree(id, queryStrings, ControllerContext);
            if (byControllerAttempt.Success)
            {
                var rootNode = await configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext);
                if (rootNode.Success == false)
                {
                    //This should really never happen if we've successfully got the children above.
                    throw new InvalidOperationException("Could not create root node for tree " + configTree.Alias);
                }

                var sectionRoot = SectionRootNode.CreateSingleTreeSectionRoot(
                    rootId, 
                    rootNode.Result.ChildNodesUrl, 
                    rootNode.Result.MenuUrl, 
                    rootNode.Result.Name,
                    byControllerAttempt.Result);

                foreach (var d in rootNode.Result.AdditionalData)
                {
                    sectionRoot.AdditionalData[d.Key] = d.Value;
                }
                return sectionRoot;

            }
            var legacyAttempt = configTree.TryLoadFromLegacyTree(id, queryStrings, Url, configTree.ApplicationAlias);
            if (legacyAttempt.Success)
            {
                var sectionRoot = SectionRootNode.CreateSingleTreeSectionRoot(
                   rootId,
                   "", //TODO: I think we'll need this in this situation!
                   Url.GetUmbracoApiService<LegacyTreeController>("GetMenu", rootId)
                        + "&parentId=" + rootId
                        + "&treeType=" + application
                        + "&section=" + application,
                   "", //TODO: I think we'll need this in this situation!
                   legacyAttempt.Result);

                
                sectionRoot.AdditionalData.Add("treeAlias", configTree.Alias);
                return sectionRoot;
            }

            throw new ApplicationException("Could not render a tree for type " + configTree.Alias);
        }
        

    }

    
}
