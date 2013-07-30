using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.businesslogic
{
    /// <summary>
    /// Identifies an application tree
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TreeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeAttribute"/> class.
        /// </summary>
        /// <param name="appAlias">The app alias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="title">The title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpen">The icon open.</param>
        /// <param name="action">The action.</param>
        /// <param name="silent">if set to <c>true</c> [silent].</param>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        public TreeAttribute(string appAlias,
            string alias,
            string title,
            string iconClosed = ".sprTreeFolder",
            string iconOpen = ".sprTreeFolder_o",
            string action = "",
            bool silent = false,
            bool initialize = true,
            int sortOrder = 0)
        {
            ApplicationAlias = appAlias;
            Alias = alias;
            Title = title;
            IconClosed = iconClosed;
            IconOpen = iconOpen;
            Action = action;
            Silent = silent;
            Initialize = initialize;
            SortOrder = sortOrder;
        }

        public string ApplicationAlias { get; private set; }
        public string Alias { get; private set; }
        public string Title { get; private set; }
        public string IconClosed { get; private set; }
        public string IconOpen { get; private set; }

        [Obsolete("This doesn't do anything")]
        public string Action { get; private set; }

        [Obsolete("This doesn't do anything")]
        public bool Silent { get; private set; }

        public bool Initialize { get; private set; }
        public int SortOrder { get; private set; }
    }
}
