using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.interfaces {
    /// <summary>
    /// IApplication is an interface for adding modules to umbraco that needs to access the event model introduced in V4.
    /// An Iapplication is automaticly invoked on application start and hooks into events.
    /// For version 5, Iapplication should also support Using it as adding complete applications to umbraco, without any database
    /// changes.
    /// </summary>
    public interface IApplication {
        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>The alias.</value>
        string Alias { get;}
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get;}
        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <value>The icon.</value>
        string Icon { get;}
        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        int SortOrder { get;}
        /// <summary>
        /// Gets a value indicating whether this <see cref="IApplication"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        bool Visible { get;}
        /// <summary>
        /// Gets the init tree alias.
        /// </summary>
        /// <value>The init tree alias.</value>
        string InitTreeAlias { get;}
        /// <summary>
        /// Gets the application trees.
        /// </summary>
        /// <value>The application trees.</value>
        List<ITree> ApplicationTrees { get;}
    }
}
