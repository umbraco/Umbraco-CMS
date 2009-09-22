using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Provides the methods required for a data access model within the LINQ to umbraco project
    /// </summary>
    /// <remarks>
    /// This base class is used when defining how a DataProvider operates against a data source (such as the umbraco.config).
    /// 
    /// It provides abstractions for all the useful operations of the DataProvider
    /// </remarks>
    public abstract class UmbracoDataProvider : IDisposable
    {
        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        /// <value>The name of the provider.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Loads the tree with the relivent DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <returns></returns>
        public abstract Tree<TDocType> LoadTree<TDocType>() where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the associated nodes with the relivent DocTypes
        /// </summary>
        /// <param name="parentNodeId">The parent node id.</param>
        /// <returns></returns>
        public abstract AssociationTree<DocTypeBase> LoadAssociation(int parentNodeId);

        /// <summary>
        /// Loads the associated nodes with the relivent DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public abstract AssociationTree<TDocType> LoadAssociation<TDocType>(IEnumerable<TDocType> nodes) where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the specified id.
        /// </summary>
        /// <typeparam name="TDocType">The type of the doc type.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public abstract TDocType Load<TDocType>(int id) where TDocType : DocTypeBase, new();

        /// <summary>
        /// Loads the ancestors.
        /// </summary>
        /// <param name="startNodeId">The start node id.</param>
        /// <returns></returns>
        public abstract IEnumerable<DocTypeBase> LoadAncestors(int startNodeId);

        #region IDisposable Members

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        protected internal virtual void Dispose(bool disposing)
        {
        }

        #endregion

        protected internal virtual void SubmitChanges()
        {
            throw new NotImplementedException("Provider \"" + this.Name + "\" does not implement a submittable pattern");
        }
    }
}
