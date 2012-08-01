using System;
using System.ComponentModel;
namespace umbraco.Linq.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDocTypeBase : INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the Ancestor or Default based on type
        /// </summary>
        /// <typeparam name="TDocType">The type of the Ancestor.</typeparam>
        /// <returns></returns>
        TDocType AncestorOrDefault<TDocType>() where TDocType : DocTypeBase;
        /// <summary>
        /// Gets the Ancestor or Default based on type
        /// </summary>
        /// <typeparam name="TDocType">The type of the Ancestor.</typeparam>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        TDocType AncestorOrDefault<TDocType>(Func<TDocType, bool> func) where TDocType : DocTypeBase;
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>The children.</value>
        AssociationTree<DocTypeBase> Children { get; }
        /// <summary>
        /// Gets or sets the create date.
        /// </summary>
        /// <value>The create date.</value>
        DateTime CreateDate { get; set; }
        /// <summary>
        /// Gets the creator user ID.
        /// </summary>
        /// <value>The creator ID.</value>
        int CreatorID { get; }
        /// <summary>
        /// Gets the name of the creator.
        /// </summary>
        /// <value>The name of the creator.</value>
        string CreatorName { get; }
        /// <summary>
        /// Gets the id of the item.
        /// </summary>
        /// <value>The id.</value>
        int Id { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        bool IsDirty { get; }
        /// <summary>
        /// Gets or sets the level of the item
        /// </summary>
        /// <value>The level.</value>
        int Level { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Obsolete("Name property is obsolete, use NodeName instead")] //this is because most people expect NodeName not Name as the property
        string Name { get; set; }
        /// <summary>
        /// Parent if current instance
        /// </summary>
        /// <typeparam name="TParent">The type of the parent.</typeparam>
        /// <returns></returns>
        TParent Parent<TParent>() where TParent : DocTypeBase, new();
        /// <summary>
        /// Gets or sets the parent node id.
        /// </summary>
        /// <value>The parent node id.</value>
        int ParentNodeId { get; set; }
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        int SortOrder { get; set; }
        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        /// <value>The template id.</value>
        int TemplateId { get; set; }
        /// <summary>
        /// Gets or sets the update date.
        /// </summary>
        /// <value>The update date.</value>
        DateTime UpdateDate { get; set; }
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        string Version { get; }
        /// <summary>
        /// Gets the writer user ID.
        /// </summary>
        /// <value>The writer ID.</value>
        int WriterID { get; }
        /// <summary>
        /// Gets the name of the writer.
        /// </summary>
        /// <value>The name of the writer.</value>
        string WriterName { get; }
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        string Path { get; set; }
    }
}
