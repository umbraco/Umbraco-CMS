using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Represents the child items of TDocTypeBase
    /// </summary>
    /// <remarks>
    /// This is used for creating a DataProvider specific child item collection of a DocType instance.
    /// 
    /// It allows a DocType to have strongly typed child associations, and accessors such as:
    /// <example>
    ///     myDocType.SomeChildTypes
    /// </example>
    /// </remarks>
    /// <typeparam name="TDocTypeBase">The type of the DocTypeBase which this association represents.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class AssociationTree<TDocTypeBase> : Tree<TDocTypeBase> where TDocTypeBase : DocTypeBase, new()
    {
        /// <summary>
        /// Gets or sets the parent node id which this AssociationTree is for
        /// </summary>
        /// <value>The parent node id.</value>
        public int ParentNodeId { get; protected set; }
    }
}
