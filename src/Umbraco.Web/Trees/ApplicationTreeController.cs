using System;
using System.Globalization;
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
using Constants = Umbraco.Core.Constants;

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
        public SectionRootNode GetApplicationTrees(string application, FormDataCollection queryStrings)
        {
            if (application == null) throw new ArgumentNullException("application");

            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);

            //find all tree definitions that have the current application alias
            var appTrees = ApplicationContext.Current.Services.ApplicationTreeService.GetApplicationTrees(application).Where(x => x.Initialize).ToArray();
            if (appTrees.Count() == 1)
            {
                return GetRootForSingleAppTree(
                    appTrees.Single(),
                    Constants.System.Root.ToString(CultureInfo.InvariantCulture),
                    queryStrings, 
                    application);
            }

            var collection = new TreeNodeCollection();
            foreach (var tree in appTrees)
            {
                //return the root nodes for each tree in the app
                var rootNode = GetRootForMultipleAppTree(tree, queryStrings);                
                collection.Add(rootNode); 
            }

            return new SectionRootNode(rootId, "")
                {
                    Children = collection,
                    IsContainer = true
                };
        }

        /// <summary>
        /// Get the root node for an application with multiple trees
        /// </summary>
        /// <param name="configTree"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        private TreeNode GetRootForMultipleAppTree(ApplicationTree configTree, FormDataCollection queryStrings)
        {
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext, Request);
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
        private SectionRootNode GetRootForSingleAppTree(ApplicationTree configTree, string id, FormDataCollection queryStrings, string application)
        {
            var rootId = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
            if (configTree == null) throw new ArgumentNullException("configTree");
            var byControllerAttempt = configTree.TryLoadFromControllerTree(id, queryStrings, ControllerContext, Request);
            if (byControllerAttempt.Success)
            {
                var rootNode = configTree.TryGetRootNodeFromControllerTree(queryStrings, ControllerContext, Request);
                if (rootNode.Success == false)
                {
                    //This should really never happen if we've successfully got the children above.
                    throw new InvalidOperationException("Could not create root node for tree " + configTree.Alias);
                }

                var sectionRoot = new SectionRootNode(
                    rootId,
                    rootNode.Result.MenuUrl)
                {
                    Title = rootNode.Result.Title,
                    Children = byControllerAttempt.Result
                };
                foreach (var d in rootNode.Result.AdditionalData)
                {
                    sectionRoot.AdditionalData[d.Key] = d.Value;
                }
                return sectionRoot;

            }
            var legacyAttempt = configTree.TryLoadFromLegacyTree(id, queryStrings, Url, configTree.ApplicationAlias);
            if (legacyAttempt.Success)
            {
                var sectionRoot = new SectionRootNode(
                    rootId,
                    Url.GetUmbracoApiService<LegacyTreeController>("GetMenu", rootId)
                        + "&parentId=" + rootId
                        + "&treeType=" + application
                        + "&section=" + application)
                {
                    Children = legacyAttempt.Result
                };
                sectionRoot.AdditionalData.Add("treeType", Type.GetType(configTree.Type).FullName);
                return sectionRoot;
            }

            throw new ApplicationException("Could not render a tree for type " + configTree.Alias);
        }
        

    }

    
}
