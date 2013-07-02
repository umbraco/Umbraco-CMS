using System;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.Http.Formatting;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Trees
{

    [PluginController("UmbracoTrees")]
    public class ApplicationTreeController : UmbracoAuthorizedApiController
    {

        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }

        /// <summary>
        /// Returns the tree nodes for an application
        /// </summary>
        /// <param name="application"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNodeCollection GetApplicationTrees(string application, FormDataCollection queryStrings)
        {
            if (application == null) throw new ArgumentNullException("application");

            //find all tree definitions that have the current application alias
            var appTrees = ApplicationContext.Current.Services.ApplicationTreeService.GetApplicationTrees(application).Where(x => x.Initialize).ToArray();
            if (appTrees.Count() == 1)
            {
                //return the nodes for the one tree assigned
                return GetNodeCollection(appTrees.Single(), "-1", queryStrings);
            }

            var collection = new TreeNodeCollection();
            foreach (var tree in appTrees)
            {
                //return the root nodes for each tree in the app
                var rootNode = GetRoot(tree, queryStrings);                
                collection.Add(rootNode); 
            }
            return collection;
        }

        ///// <summary>
        ///// Returns the tree data for a specific tree for the children of the id
        ///// </summary>
        ///// <param name="treeType"></param>
        ///// <param name="id"></param>
        ///// <param name="queryStrings"></param>
        ///// <returns></returns>
        //[HttpQueryStringFilter("queryStrings")]
        //public TreeNodeCollection GetTreeData(string treeType, string id, FormDataCollection queryStrings)
        //{
        //    if (treeType == null) throw new ArgumentNullException("treeType");

        //    //get the configured tree
        //    var foundConfigTree = ApplicationTreeCollection.GetByAlias(treeType);
        //    if (foundConfigTree == null) 
        //        throw new InstanceNotFoundException("Could not find tree of type " + treeType + " in the trees.config");

        //    return GetNodeCollection(foundConfigTree, id, queryStrings);
        //}

        private TreeNode GetRoot(ApplicationTree configTree, FormDataCollection queryStrings)
        {
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext, Request);
            if (byControllerAttempt.Success)
            {
                return byControllerAttempt.Result;
            }
            var legacyAttempt = configTree.TryGetRootNodeFromLegacyTree(queryStrings, Url);
            if (legacyAttempt.Success)
            {
                return legacyAttempt.Result;
            }

            throw new ApplicationException("Could not get root node for tree type " + configTree.Alias);
        }

        /// <summary>
        /// Get the node collection for the tree, try loading from new controllers first, then from legacy trees
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private TreeNodeCollection GetNodeCollection(ApplicationTree configTree, string id, FormDataCollection queryStrings)
        {
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = configTree.TryLoadFromControllerTree(id, queryStrings, ControllerContext, Request);
            if (byControllerAttempt.Success)
            {
                return byControllerAttempt.Result;
            }
            var legacyAttempt = configTree.TryLoadFromLegacyTree(id, queryStrings, Url);
            if (legacyAttempt.Success)
            {
                return legacyAttempt.Result;
            }

            throw new ApplicationException("Could not render a tree for type " + configTree.Alias);
        }
        

    }

    
}
