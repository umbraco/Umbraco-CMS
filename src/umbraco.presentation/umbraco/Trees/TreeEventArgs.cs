using System;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.presentation.Trees
{
    /// <summary>
    /// 
    /// </summary>
    public class TreeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the tree.
        /// </summary>
        /// <value>The tree.</value>
        public XmlTree Tree { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeEventArgs"/> class.
        /// </summary>
        /// <param name="tree">The tree which the event is for.</param>
        public TreeEventArgs(XmlTree tree)
        {
            this.Tree = tree;
        }
    }
}
