using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace umbraco.Linq.Core.Node
{
    /// <summary>
    /// Represents a collection of TDocTypeBase retrieved from the umbraco XML cache
    /// </summary>
    /// <typeparam name="TDocTypeBase">The type of the doc type base.</typeparam>
    public sealed class NodeTree<TDocTypeBase> : Tree<TDocTypeBase> where TDocTypeBase : DocTypeBase, new()
    {
        private object lockObject = new object();

        private NodeDataProvider _provider;

        private List<TDocTypeBase> _nodes;

        internal NodeTree(NodeDataProvider provider)
        {
            this._provider = provider;
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public override UmbracoDataProvider Provider
        {
            get
            {
                return this._provider;
            }
            protected set
            {
                var nodeProvider = value as NodeDataProvider;
                if (nodeProvider == null)
                {
                    throw new ArgumentException("Value must be of type NodeDataProvider");
                }
                this._provider = nodeProvider;
            }
        }

        /// <summary>
        /// Indicates that the NodeTree is ReadOnly
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
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<TDocTypeBase> GetEnumerator()
        {
            if (this._nodes == null)
            {
                this._nodes = new List<TDocTypeBase>();
                var rawNodes = this._provider.Xml.Descendants("node").Where(x => ReflectionAssistance.CompareByAlias(typeof(TDocTypeBase), x));

                lock (lockObject)
                {
                    foreach (XElement n in rawNodes)
                    {
                        var dt = new TDocTypeBase();
                        dt.LoadFromXml(n);

                        dt.IsDirty = false;
                        dt.Provider = this._provider;

                        this._nodes.Add(dt);
                    }
                }
            }
            return this._nodes.GetEnumerator();
        }

        /// <summary>
        /// Reloads the cache for the particular NodeTree
        /// </summary>
        public override void ReloadCache()
        {
            this._provider.CheckDisposed();

            var attr = ReflectionAssistance.GetumbracoInfoAttribute(typeof(TDocTypeBase));
            this._provider.SetupNodeTree<TDocTypeBase>(attr);
        }

        public override void InsertOnSubmit(TDocTypeBase item)
        {
            throw new NotImplementedException("The NodeTree does not support Inserting items");
        }

        public override void InsertAllOnSubmit(IEnumerable<TDocTypeBase> items)
        {
            throw new NotImplementedException("The NodeTree does not support Inserting items");
        }

        public override void DeleteOnSubmit(TDocTypeBase itemm)
        {
            throw new NotImplementedException("The NodeTree does not support Deleting items");
        }

        public override void DeleteAllOnSubmit(IEnumerable<TDocTypeBase> items)
        {
            throw new NotImplementedException("The NodeTree does not support Deleting items");
        }
    }
}
