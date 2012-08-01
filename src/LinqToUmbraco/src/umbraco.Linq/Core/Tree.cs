using System.Collections.Generic;
using System.Collections;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Represents a collection within DataProvider of a DocType
    /// </summary>
    /// <remarks>
    /// Similar to the implementation of <see cref="System.Data.Linq.Table&lt;TEntity&gt;"/>, 
    /// providing a single collection which represents all instances of the given type within the DataProvider.
    /// 
    /// Implementers of this type will need to provide a manner of retrieving the TDocType from the DataProvider
    /// </remarks>
    /// <typeparam name="TDocType">The type of the DocType.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class Tree<TDocType> : IContentTree, IEnumerable<TDocType>, IEnumerable
        where TDocType : DocTypeBase, new()
    {
        #region IContentTree Members

        /// <summary>
        /// Gets the <see cref="umbracoDataProvider"/> Provider associated with this instance
        /// </summary>
        /// <value>The provider.</value>
        public abstract UmbracoDataProvider Provider { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is read only. The collection is not ReadOnly by default
        /// </summary>
        /// <value>
        /// 	<c>false</c> to indicate that this collection isn't read-only
        /// </value>
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public abstract void ReloadCache();

        #endregion

        #region IEnumerable<TDocType> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public abstract IEnumerator<TDocType> GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Insert an item on submit of the DataContext
        /// </summary>
        /// <param name="item">The item.</param>
        public abstract void InsertOnSubmit(TDocType item);
        /// <summary>
        /// Insert a collection of items on submit of the DataContext
        /// </summary>
        /// <param name="items">The items.</param>
        public abstract void InsertAllOnSubmit(IEnumerable<TDocType> items);
        /// <summary>
        /// Deletes an item on submit of the DataContext
        /// </summary>
        /// <param name="itemm">The itemm.</param>
        public abstract void DeleteOnSubmit(TDocType itemm);
        /// <summary>
        /// Deletes a collection of items on submit of the DataContext
        /// </summary>
        /// <param name="items">The items.</param>
        public abstract void DeleteAllOnSubmit(IEnumerable<TDocType> items);
    }
}
