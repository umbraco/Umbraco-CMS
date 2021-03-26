using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    /// <summary>
    /// A notification that allows developers to modify the menu that is being rendered
    /// </summary>
    /// <remarks>
    /// Developers can add/remove/replace/insert/update/etc... any of the tree items in the collection.
    /// </remarks>
    public class MenuRenderingNotification : INotification
    {
        /// <summary>
        /// The tree node id that the menu is rendering for
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// The alias of the tree the menu is rendering for
        /// </summary>
        public string TreeAlias { get; }

        /// <summary>
        /// The menu being rendered
        /// </summary>
        public MenuItemCollection Menu { get; }

        /// <summary>
        /// The query string of the current request
        /// </summary>
        public FormCollection QueryString { get; }

        public MenuRenderingNotification(string nodeId, MenuItemCollection menu, FormCollection queryString, string treeAlias)
        {
            NodeId = nodeId;
            Menu = menu;
            QueryString = queryString;
            TreeAlias = treeAlias;
        }
    }
}
