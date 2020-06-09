﻿using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    // Migrated to .NET Core
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

        public MenuRenderingEventArgs(string nodeId, MenuItemCollection menu, FormDataCollection queryStrings)
            : base(queryStrings)
        {
            NodeId = nodeId;
            Menu = menu;
        }
    }
}
