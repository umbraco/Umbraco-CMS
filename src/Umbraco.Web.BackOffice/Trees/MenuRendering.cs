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
    public class MenuRendering : INotification
    {
        public MenuRendering(string nodeId, MenuItemCollection menu, FormCollection queryStrings)
        {
            NodeId = nodeId;
            Menu = menu;
            QueryStrings = queryStrings;
        }

        /// <summary>
        /// The tree node id that the menu is rendering for
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// The menu being rendered
        /// </summary>
        public MenuItemCollection Menu { get; }

        public FormCollection QueryStrings { get; }


    }
}
