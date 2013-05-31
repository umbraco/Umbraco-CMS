using System;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.Http.Formatting;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{

    //NOTE: We will of course have to authorized this but changing the base class once integrated

    [PluginController("UmbracoTrees")]
    public class ApplicationTreeApiController : UmbracoApiController //UmbracoAuthorizedApiController
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

        ///// <summary>
        ///// Returns the tree nodes for an application
        ///// </summary>
        ///// <param name="application"></param>
        ///// <param name="queryStrings"></param>
        ///// <returns></returns>
        //[HttpQueryStringFilter("queryStrings")]
        //public TreeNodeCollection GetApplicationTrees(string application, FormDataCollection queryStrings)
        //{
        //    if (application == null) throw new ArgumentNullException("application");
            
        //    //find all tree definitions that have the current application alias
        //    var appTrees = ApplicationTree.getApplicationTree(application);
        //    if (appTrees.Count() == 1)
        //    {
        //        return GetNodeCollection(appTrees.Single(), "-1", queryStrings);
        //    }
        //    foreach (var tree in appTrees)
        //    {
                
        //    }
        //}

        /// <summary>
        /// Returns the tree data for a specific tree for the children of the id
        /// </summary>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNodeCollection GetTreeData(string treeType, string id, FormDataCollection queryStrings)
        {
            if (treeType == null) throw new ArgumentNullException("treeType");

            //get the configured tree
            var foundConfigTree = ApplicationTree.getByAlias(treeType);
            if (foundConfigTree == null) 
                throw new InstanceNotFoundException("Could not find tree of type " + treeType + " in the trees.config");

            return GetNodeCollection(foundConfigTree, id, queryStrings);
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
            var byControllerAttempt = TryLoadFromControllerTree(configTree, id, queryStrings);
            if (byControllerAttempt.Success)
            {
                return byControllerAttempt.Result;
            }
            var legacyAttempt = TryLoadFromLegacyTree(configTree, id, queryStrings);
            if (legacyAttempt.Success)
            {
                return legacyAttempt.Result;
            }

            throw new ApplicationException("Could not render a tree for type " + configTree.Alias);
        }

        private Attempt<TreeNodeCollection> TryLoadFromControllerTree(ApplicationTree appTree, string id, FormDataCollection formCollection)
        {
            //get reference to all TreeApiControllers
            var controllerTrees = UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers
                                                              .Where(TypeHelper.IsTypeAssignableFrom<TreeApiController>)
                                                              .ToArray();

            //find the one we're looking for
            var foundControllerTree = controllerTrees.FirstOrDefault(x => x.GetFullNameWithAssembly() == appTree.Type);
            if (foundControllerTree == null)
            {
                return new Attempt<TreeNodeCollection>(new InstanceNotFoundException("Could not find tree of type " + appTree.Type + " in any loaded DLLs"));
            }

            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeApiController)DependencyResolver.Current.GetService(foundControllerTree);
            instance.ControllerContext = ControllerContext;
            instance.Request = Request;

            //return it's data
            return new Attempt<TreeNodeCollection>(true, instance.GetNodes(id, formCollection));
        }

        private Attempt<TreeNodeCollection> TryLoadFromLegacyTree(ApplicationTree appTree, string id, FormDataCollection formCollection)
        {
            //This is how the legacy trees worked....
            var treeDef = TreeDefinitionCollection.Instance.FindTree(appTree.Alias);
            if (treeDef == null)
            {
                return new Attempt<TreeNodeCollection>(new InstanceNotFoundException("Could not find tree of type " + appTree.Alias));
            }

            var bTree = treeDef.CreateInstance();
            var treeParams = new TreeParams();

            //we currently only support an integer id or a string id, we'll refactor how this works
            //later but we'll get this working first
            int startId;
            if (int.TryParse(id, out startId))
            {
                treeParams.StartNodeID = startId;
            }
            else
            {
                treeParams.NodeKey = id;
            }
            var xTree = new XmlTree();
            bTree.SetTreeParameters(treeParams);
            bTree.Render(ref xTree);

            return new Attempt<TreeNodeCollection>(true, LegacyTreeDataAdapter.ConvertFromLegacy(xTree));
        }

        //Temporary, but necessary until we refactor trees in general
        internal class TreeParams : ITreeService
        {
            public string NodeKey { get; set; }
            public int StartNodeID { get; set; }
            public bool ShowContextMenu { get; set; }
            public bool IsDialog { get; set; }
            public TreeDialogModes DialogMode { get; set; }
            public string FunctionToCall { get; set; }
        }

    }
}
