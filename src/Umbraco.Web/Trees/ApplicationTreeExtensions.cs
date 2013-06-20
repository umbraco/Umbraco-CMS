using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Instrumentation;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.WebApi;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using ApplicationTree = Umbraco.Core.Trees.ApplicationTree;
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace Umbraco.Web.Trees
{
    internal static class ApplicationTreeExtensions
    {

        private static Attempt<Type> TryGetControllerTree(this ApplicationTree appTree)
        {
            //get reference to all TreeApiControllers
            var controllerTrees = UmbracoApiControllerResolver.Current.RegisteredUmbracoApiControllers
                                                              .Where(TypeHelper.IsTypeAssignableFrom<TreeApiController>)
                                                              .ToArray();

            //find the one we're looking for
            var foundControllerTree = controllerTrees.FirstOrDefault(x => x.GetFullNameWithAssembly() == appTree.Type);
            if (foundControllerTree == null)
            {
                return new Attempt<Type>(new InstanceNotFoundException("Could not find tree of type " + appTree.Type + " in any loaded DLLs"));
            }
            return new Attempt<Type>(true, foundControllerTree);
        }

        internal static Attempt<TreeNode> TryGetRootNodeFromControllerTree(this ApplicationTree appTree, FormDataCollection formCollection, HttpControllerContext controllerContext, HttpRequestMessage request)
        {
            var foundControllerTreeAttempt = appTree.TryGetControllerTree();
            if (foundControllerTreeAttempt.Success == false)
            {
                return new Attempt<TreeNode>(foundControllerTreeAttempt.Error);
            }
            var foundControllerTree = foundControllerTreeAttempt.Result;
            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeApiController)DependencyResolver.Current.GetService(foundControllerTree);
            instance.ControllerContext = controllerContext;
            instance.Request = request;
            //return the root
            var nodes = instance.GetNodes(Constants.System.Root.ToString(CultureInfo.InvariantCulture), formCollection);
            return nodes.Any() == false 
                ? new Attempt<TreeNode>(new InvalidOperationException("Could not return a root node for tree " + appTree.Type)) 
                : new Attempt<TreeNode>(true, nodes.First());
        }

        internal static  Attempt<TreeNodeCollection> TryLoadFromControllerTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, HttpControllerContext controllerContext, HttpRequestMessage request)
        {
            var foundControllerTreeAttempt = appTree.TryGetControllerTree();
            if (foundControllerTreeAttempt.Success == false)
            {
                return new Attempt<TreeNodeCollection>(foundControllerTreeAttempt.Error);
            }
            var foundControllerTree = foundControllerTreeAttempt.Result;

            //instantiate it, since we are proxying, we need to setup the instance with our current context
            var instance = (TreeApiController)DependencyResolver.Current.GetService(foundControllerTree);
            instance.ControllerContext = controllerContext;
            instance.Request = request;
            //return it's data
            return new Attempt<TreeNodeCollection>(true, instance.GetNodes(id, formCollection));
        }

        internal static Attempt<TreeNode> TryGetRootNodeFromLegacyTree(this ApplicationTree appTree, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            var treeDefAttempt = appTree.TryGetLegacyTreeDef();
            if (treeDefAttempt.Success == false)
            {
                return new Attempt<TreeNode>(treeDefAttempt.Error);
            }
            var treeDef = treeDefAttempt.Result;
            var bTree = treeDef.CreateInstance();
            var treeParams = new LegacyTreeParams(formCollection);
            bTree.SetTreeParameters(treeParams);
            return new Attempt<TreeNode>(true, LegacyTreeDataAdapter.ConvertFromLegacy(bTree.RootNode, urlHelper));
        }

        private static Attempt<TreeDefinition> TryGetLegacyTreeDef(this ApplicationTree appTree)
        {
            //This is how the legacy trees worked....
            var treeDef = TreeDefinitionCollection.Instance.FindTree(appTree.Alias);
            return treeDef == null 
                ? new Attempt<TreeDefinition>(new InstanceNotFoundException("Could not find tree of type " + appTree.Alias)) 
                : new Attempt<TreeDefinition>(true, treeDef);
        }

        internal static Attempt<TreeNodeCollection> TryLoadFromLegacyTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            var treeDefAttempt = appTree.TryGetLegacyTreeDef();            
            if (treeDefAttempt.Success == false)
            {
                return new Attempt<TreeNodeCollection>(treeDefAttempt.Error);
            }            
            var treeDef = treeDefAttempt.Result;
            //This is how the legacy trees worked....
            var bTree = treeDef.CreateInstance();
            var treeParams = new LegacyTreeParams(formCollection);

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

            return new Attempt<TreeNodeCollection>(true, LegacyTreeDataAdapter.ConvertFromLegacy(xTree, urlHelper));
        }

    }
}
