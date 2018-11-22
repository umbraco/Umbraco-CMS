using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.BusinessLogic;
using umbraco.interfaces;

namespace umbraco.cms.presentation.Trees
{

    /// <summary>
    /// Defines the entire structure of an application tree including it's Type, a reference to it's ApplicationTree object, and a reference 
    /// to it's Application object. Tree Definitions are based on defining a database in the umbracoAppTree database. Any tree defined in this table 
    /// that is of an ITree type, it will be found and can be instantiated by this class. Any ITree that is not defined in the database will
    /// need to be instantiated with it's own tree constructor.
    /// </summary>
    public class TreeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeDefinition"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="tree">The tree.</param>
        /// <param name="app">The app.</param>
        public TreeDefinition(Type type, ApplicationTree tree, Application app)
        {
            m_treeType = type;
            m_tree = tree;
            m_app = app;
        }

        private Type m_treeType;
        private ApplicationTree m_tree;
        private Application m_app;

        /// <summary>
        /// Returns a new instance of a BaseTree based on this Tree Definition
        /// </summary>
        public BaseTree CreateInstance()
        {       
            //create the tree instance
            ITree typeInstance = CreateTreeInstance(m_treeType, m_app.alias);

            if (typeInstance != null)
            {
                //convert to BaseTree
                return (BaseTree.IsBaseTree(typeInstance) ? typeInstance as BaseTree : BaseTree.FromITree(typeInstance, m_tree.Alias, m_app.alias, m_tree.IconClosed, m_tree.IconOpened, m_tree.Action));                    
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the type of the tree.
        /// </summary>
        /// <value>The type of the tree.</value>
        public Type TreeType
        {
            get { return m_treeType; }
            set { m_treeType = value; }
        }

        /// <summary>
        /// Gets or sets the tree.
        /// </summary>
        /// <value>The tree.</value>
        public ApplicationTree Tree
        {
            get { return m_tree; }
            set { m_tree = value; }
        }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>The app.</value>
        public Application App
        {
            get { return m_app; }
            set { m_app = value; }
        }

        /// <summary>
        /// Creates an ITree instance.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="appAlias">The app alias.</param>
        /// <returns></returns>
        public static ITree CreateTreeInstance(Type tree, string appAlias)
        {
            ITree typeInstance;
            //call the correct constructor
            if (typeof(BaseTree).IsAssignableFrom(tree))
                typeInstance = Activator.CreateInstance(tree, new object[] { appAlias }) as ITree; //the BaseTree constructor
            else
                typeInstance = CreateTreeInstance(tree);
            return typeInstance;
        }

        private static ITree CreateTreeInstance(Type tree)
        {
            ITree typeInstance = Activator.CreateInstance(tree) as ITree; //an empty constructor (ITree)
            return typeInstance;
        }

    }
}
