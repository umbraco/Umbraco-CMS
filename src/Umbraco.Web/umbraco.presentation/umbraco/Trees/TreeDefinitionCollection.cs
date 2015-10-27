using System;
using System.Data;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using umbraco.interfaces;
using umbraco.BusinessLogic.Utils;
using umbraco.BusinessLogic;
using umbraco.BasePages;

namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// A collection of TreeDefinitions found in any loaded assembly.
    /// </summary>
    public class TreeDefinitionCollection : List<TreeDefinition>
    {

        //create singleton
        private static readonly TreeDefinitionCollection instance = new TreeDefinitionCollection();

        private static readonly object Locker = new object();
        private static volatile bool _ensureTrees = false;

        public static TreeDefinitionCollection Instance
        {
            get 
            {
				instance.EnsureTreesRegistered();
                return instance; 
            }
        }

        /// <summary>
        /// Find the TreeDefinition object based on the ITree
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public TreeDefinition FindTree(ITree tree)
        {
			EnsureTreesRegistered();

            var foundTree = this.Find(
            	t => t.TreeType == tree.GetType()
            	);
            if (foundTree != null)
                return foundTree;

            return null;
        }

        /// <summary>
        /// Finds the TreeDefinition with the generic type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TreeDefinition FindTree<T>() where T : ITree
        {
			EnsureTreesRegistered();

            var foundTree = this.Find(
               delegate(TreeDefinition t)
               {
                   // zb-00002 #29929 : use IsAssignableFrom instead of Equal, otherwise you can't override built-in
                   // trees because for ex. PermissionEditor.aspx.cs OnInit calls FindTree<loadContent>()
                   return typeof(T).IsAssignableFrom(t.TreeType);
               }
           );
            if (foundTree != null)
                return foundTree;

            return null;
        }

        /// <summary>
        /// Return the TreeDefinition object based on the tree alias and application it belongs to
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public TreeDefinition FindTree(string alias)
        {
			EnsureTreesRegistered();

            var foundTree = this.Find(
            	t => t.Tree.Alias.ToLower() == alias.ToLower()
            	);
            if (foundTree != null)
                return foundTree;

            return null;
        }

        /// <summary>
        /// Return a list of TreeDefinition's with the appAlias specified
        /// </summary>
        /// <param name="appAlias"></param>
        /// <returns></returns>
        public List<TreeDefinition> FindTrees(string appAlias)
        {
			EnsureTreesRegistered();

            return this.FindAll(
            	tree => (tree.App != null && tree.App.alias.ToLower() == appAlias.ToLower())
            	);
        }

        /// <summary>
        /// Return a list of TreeDefinition's with the appAlias specified
        /// </summary>
        /// <param name="appAlias"></param>
        /// <returns></returns>
        public List<TreeDefinition> FindActiveTrees(string appAlias)
        {
			EnsureTreesRegistered();

            return this.FindAll(
            	tree => (tree.App != null && tree.App.alias.ToLower() == appAlias.ToLower() && tree.Tree.Initialize)
            	);
        }

        public void ReRegisterTrees()
        {
            //clears the trees/flag so that they are lazily refreshed on next access
            lock (Locker)
            {
                this.Clear();
                _ensureTrees = false;
            }
        }

        /// <summary>
        /// Finds all instances of ITree in loaded assemblies, then finds their associated ApplicationTree and Application objects
        /// and stores them together in a TreeDefinition class and adds the definition to our list.
        /// This will also store an instance of each tree object in the TreeDefinition class which should be 
        /// used when referencing all tree classes.
        /// </summary>
        private void EnsureTreesRegistered()
        {
            if (_ensureTrees == false)
            {
                lock (Locker)
                {
                    if (_ensureTrees == false)
                    {

                        var foundITrees = PluginManager.Current.ResolveTrees();

                        var objTrees = ApplicationTree.getAll();
                        var appTrees = new List<ApplicationTree>();
                        appTrees.AddRange(objTrees);

                        var apps = Application.getAll();

                        foreach (var type in foundITrees)
                        {

                            //find the Application tree's who's combination of assembly name and tree type is equal to 
                            //the Type that was found's full name.
                            //Since a tree can exist in multiple applications we'll need to register them all.

                            //The logic of this has changed in 6.0: http://issues.umbraco.org/issue/U4-1360
                            // we will support the old legacy way but the normal way is to match on assembly qualified names

                            var appTreesForType = appTrees.FindAll(
                                tree =>
                                {
                                    //match the type on assembly qualified name if the assembly attribute is empty or if the
                                    // tree type contains a comma (meaning it is assembly qualified)
                                    if (tree.AssemblyName.IsNullOrWhiteSpace() || tree.Type.Contains(","))
                                    {
                                        return tree.GetRuntimeType() == type;
                                    }

                                    //otherwise match using legacy match rules
                                    return (string.Format("{0}.{1}", tree.AssemblyName, tree.Type).InvariantEquals(type.FullName));
                                }
                                );

                            foreach (var appTree in appTreesForType)
                            {
                                //find the Application object whos name is the same as our appTree ApplicationAlias
                                var app = apps.Find(
                                    a => (a.alias == appTree.ApplicationAlias)
                                    );

                                var def = new TreeDefinition(type, appTree, app);
                                this.Add(def);
                            }
                        }
                        //sort our trees with the sort order definition
                        this.Sort((t1, t2) => t1.Tree.SortOrder.CompareTo(t2.Tree.SortOrder));

                        _ensureTrees = true;
                    }
                }
            }


        }
       
    }
}
