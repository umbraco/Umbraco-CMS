using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace umbraco.Linq.Core.Node
{
    /// <summary>
    /// Represents a collection of TDocTypeBase retrieved from the umbraco XML cache which are direct children of a node
    /// </summary>
    /// <typeparam name="TDocTypeBase">The type of the doc type base.</typeparam>
    public sealed class NodeAssociationTree<TDocTypeBase> : AssociationTree<TDocTypeBase> where TDocTypeBase : DocTypeBase, new()
    {
        private IEnumerable<TDocTypeBase> _nodes;

        internal NodeAssociationTree(IEnumerable<TDocTypeBase> nodes)
        {
            this._nodes = new List<TDocTypeBase>();
            this._nodes = nodes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeAssociationTree&lt;TDocTypeBase&gt;"/> class for a particular tree section
        /// </summary>
        /// <param name="parentNodeId">The parent node id to start from.</param>
        /// <param name="provider">The NodeDataProvider to link the tree with.</param>
        public NodeAssociationTree(int parentNodeId, NodeDataProvider provider)
        {
            this.Provider = provider;
            this.ParentNodeId = parentNodeId;            
        }

        /// <summary>
        /// Gets the enumerator for this Tree collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<TDocTypeBase> GetEnumerator()
        {
            if (this._nodes == null) //first access, otherwise it'd be cached
            {
                LoadNodes();
            }
            return this._nodes.GetEnumerator();
        }

        private void LoadNodes()
        {
            var provider = this.Provider as NodeDataProvider;

            provider.CheckDisposed();

            var rawNodes = provider.Xml.Descendants("node")
            .Where(x => (int)x.Attribute("id") == this.ParentNodeId)
            .Single()
            .Elements("node")
            ;
            this._nodes = provider.DynamicNodeCreation(rawNodes).Cast<TDocTypeBase>(); //drop is back to the type which was asked for
        }

        /// <summary>
        /// Indicates that the NodeAssociationTree is ReadOnly
        /// </summary>
        /// <value>
        /// 	<c>true</c>
        /// </value>
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the DataProvider associated with this Tree
        /// </summary>
        /// <value>The provider.</value>
        public override umbracoDataProvider Provider { get; protected set; }

        public override void ReloadCache()
        {
            this.LoadNodes();
        }
    }
}
