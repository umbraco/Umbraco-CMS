using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    public class MenuRenderingEventArgs : TreeRenderingEventArgs
    {
        /// <summary>
        /// The tree node id that the menu is rendering for
        /// </summary>
        public string NodeId { get; private set; }

        /// <summary>
        /// The menu being rendered
        /// </summary>
        public MenuItemCollection Menu { get; private set; }

        public MenuRenderingEventArgs(string nodeId, MenuItemCollection menu, FormCollection queryStrings)
            : base(queryStrings)
        {
            NodeId = nodeId;
            Menu = menu;
        }
    }
}
