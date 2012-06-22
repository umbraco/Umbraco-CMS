using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
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
        private TreeDefinitionCollection()
        {
            RegisterTrees();
        }
        public static TreeDefinitionCollection Instance
        {
            get 
            {
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
            TreeDefinition foundTree = this.Find(
               delegate(TreeDefinition t)
               {
                   return t.TreeType.Equals(tree.GetType());
               }
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
            TreeDefinition foundTree = this.Find(
               delegate(TreeDefinition t)
               {
                   // zb-00002 #29929 : use IsAssignableFrom instead of Equal, otherwise you can't override build-in
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
        /// <param name="appAlias"></param>
        /// <returns></returns>
        public TreeDefinition FindTree(string alias)
        {
            TreeDefinition foundTree = this.Find(
                delegate(TreeDefinition t)
                {
                    return t.Tree.Alias.ToLower() == alias.ToLower();
                }
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
            return this.FindAll(
                delegate(TreeDefinition tree)
                {
                    return (tree.App != null && tree.App.alias.ToLower() == appAlias.ToLower());
                }
            );
        }

        /// <summary>
        /// Return a list of TreeDefinition's with the appAlias specified
        /// </summary>
        /// <param name="appAlias"></param>
        /// <returns></returns>
        public List<TreeDefinition> FindActiveTrees(string appAlias)
        {
            return this.FindAll(
                delegate(TreeDefinition tree)
                {
                    return (tree.App != null && tree.App.alias.ToLower() == appAlias.ToLower() && tree.Tree.Initialize);
                }
            );
        }

        public void ReRegisterTrees()
        {
            this.Clear();
            RegisterTrees();
        }

        /// <summary>
        /// Finds all instances of ITree in loaded assemblies, then finds their associated ApplicationTree and Application objects
        /// and stores them together in a TreeDefinition class and adds the definition to our list.
        /// This will also store an instance of each tree object in the TreeDefinition class which should be 
        /// used when referencing all tree classes.
        /// </summary>
        private void RegisterTrees()
        {

            if (this.Count > 0)
                return;

            List<Type> foundITrees = TypeFinder.FindClassesOfType<ITree>();

            ApplicationTree[] objTrees = ApplicationTree.getAll();
            List<ApplicationTree> appTrees = new List<ApplicationTree>();
            appTrees.AddRange(objTrees);

            List<Application> apps = Application.getAll();

            foreach (Type type in foundITrees)
            {

                //find the Application tree's who's combination of assembly name and tree type is equal to 
                //the Type that was found's full name.
				//Since a tree can exist in multiple applications we'll need to register them all.
                List<ApplicationTree> appTreesForType = appTrees.FindAll(
                    delegate(ApplicationTree tree)
                    {
                        return (string.Format("{0}.{1}", tree.AssemblyName, tree.Type) == type.FullName);
                    }
                );

				if (appTreesForType != null)
                {
					foreach (ApplicationTree appTree in appTreesForType)
					{
						//find the Application object whos name is the same as our appTree ApplicationAlias
						Application app = apps.Find(
							 delegate(Application a)
							 {
								 return (a.alias == appTree.ApplicationAlias);
							 }
						);

						TreeDefinition def = new TreeDefinition(type, appTree, app);
						this.Add(def);
					}

                }
            }
            //sort our trees with the sort order definition
            this.Sort(delegate(TreeDefinition t1, TreeDefinition t2)
              {
                  return t1.Tree.SortOrder.CompareTo(t2.Tree.SortOrder);
              });
        }
       
    }
}
