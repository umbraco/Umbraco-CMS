using System;
using System.Collections.Generic;
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
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace Umbraco.Web.Trees
{
    internal static class ApplicationTreeExtensions
    {

        internal static  Attempt<TreeNodeCollection> TryLoadFromControllerTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, HttpControllerContext controllerContext, HttpRequestMessage request)
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
            instance.ControllerContext = controllerContext;
            instance.Request = request;
            //return it's data
            return new Attempt<TreeNodeCollection>(true, instance.GetNodes(id, formCollection));
        }

        internal static Attempt<TreeNodeCollection> TryLoadFromLegacyTree(this ApplicationTree appTree, string id, FormDataCollection formCollection, UrlHelper urlHelper)
        {
            //This is how the legacy trees worked....
            var treeDef = TreeDefinitionCollection.Instance.FindTree(appTree.Alias);
            if (treeDef == null)
            {
                return new Attempt<TreeNodeCollection>(new InstanceNotFoundException("Could not find tree of type " + appTree.Alias));
            }

            var bTree = treeDef.CreateInstance();
            var treeParams = new ApplicationTreeApiController.TreeParams();

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
