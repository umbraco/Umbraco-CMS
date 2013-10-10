using System;
using System.Diagnostics;

namespace Umbraco.Core.Models
{
    [DebuggerDisplay("Tree - {Title} ({ApplicationAlias})")]
    public class ApplicationTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        public ApplicationTree() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="alias">The tree alias.</param>
        /// <param name="title">The tree title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpened">The icon opened.</param>
        /// <param name="type">The tree type.</param>
        public ApplicationTree(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        {
            this.Initialize = initialize;
            this.SortOrder = sortOrder;
            this.ApplicationAlias = applicationAlias;
            this.Alias = alias;
            this.Title = title;
            this.IconClosed = iconClosed;
            this.IconOpened = iconOpened;
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> should initialize.
        /// </summary>
        /// <value><c>true</c> if initialize; otherwise, <c>false</c>.</value>
        public bool Initialize { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets the application alias.
        /// </summary>
        /// <value>The application alias.</value>
        public string ApplicationAlias { get; private set; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets the tree title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the icon closed.
        /// </summary>
        /// <value>The icon closed.</value>
        public string IconClosed { get; set; }

        /// <summary>
        /// Gets or sets the icon opened.
        /// </summary>
        /// <value>The icon opened.</value>
        public string IconOpened { get; set; }

        /// <summary>
        /// Gets or sets the tree type assembly name.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        private Type _runtimeType;

        /// <summary>
        /// Returns the CLR type based on it's assembly name stored in the config
        /// </summary>
        /// <returns></returns>
        public Type GetRuntimeType()
        {
            return _runtimeType ?? (_runtimeType = System.Type.GetType(Type));
        }
            

    }
}