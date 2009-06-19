using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using umbraco.Linq.Core.Node;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// The umbracoDataContext which handles the interaction with an <see cref="umbracoDataProvider"/>
    /// </summary>
    public abstract class UmbracoDataContext : IDisposable
    {
        #region Privates
        private UmbracoDataProvider _dataProvider;
        #endregion

        /// <summary>
        /// Loads the tree of umbraco items.
        /// </summary>
        /// <typeparam name="TDocTypeBase">The type of the DocTypeBase.</typeparam>
        /// <returns>Collection of umbraco items</returns>
        /// <exception cref="System.ObjectDisposedException">If the DataContext has been disposed of</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        protected Tree<TDocTypeBase> LoadTree<TDocTypeBase>() where TDocTypeBase : DocTypeBase, new()
        {
            CheckDisposed();
            return this._dataProvider.LoadTree<TDocTypeBase>();
        }

        /// <summary>
        /// Gets the data provider.
        /// </summary>
        /// <value>The data provider.</value>
        public UmbracoDataProvider DataProvider
        {
            get
            {
                return this._dataProvider;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="umbracoDataContext"/> class, using a <see cref="umbraco.Linq.Core.Node.NodeDataProvider"/> data provider with the connection string from the umbraco config
        /// </summary>
        protected UmbracoDataContext() : this(new NodeDataProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="umbracoDataContext"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider to use within the DataContext.</param>
        protected UmbracoDataContext(UmbracoDataProvider dataProvider)
        {
            this._dataProvider = dataProvider;
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                if (this._dataProvider != null)
                {
                    this._dataProvider.Dispose(true);
                }

                this._disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        #endregion
    }
}
