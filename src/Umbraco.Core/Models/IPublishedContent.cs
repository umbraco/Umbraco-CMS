using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models
{
	/// <summary>
	/// Represents a cached content.
	/// </summary>
	/// <remarks>
    /// <para>SD: A replacement for INode which needs to occur since INode doesn't contain the document type alias
    /// and INode is poorly formatted with mutable properties (i.e. Lists instead of IEnumerable).</para>
    /// <para>Stephan: initially, that was for cached published content only. Now, we're using it also for
    /// cached preview (so, maybe unpublished) content. A better name would therefore be ICachedContent, as
    /// has been suggested. However, can't change now. Maybe in v7?</para>
	/// </remarks>	
    public interface IPublishedContent
    {
        #region ContentSet

        // Because of http://issues.umbraco.org/issue/U4-1797 and in order to implement
        // Index() and methods that derive from it such as IsFirst(), IsLast(), etc... all
        // content items must know about their containing content set.

        /// <summary>
        /// Gets the content set to which the content belongs.
        /// </summary>
        /// <remarks>The default set consists in the siblings of the content (including the content 
        /// itself) ordered by <c>sortOrder</c>.</remarks>
        IEnumerable<IPublishedContent> ContentSet { get; }

        #endregion

        #region ContentType

        /// <summary>
        /// Gets the content type.
        /// </summary>
        PublishedContentType ContentType { get; }

        #endregion

        #region Content

        int Id { get; }
		int TemplateId { get; }
		int SortOrder { get; }
		string Name { get; }
		string UrlName { get; }
		string DocumentTypeAlias { get; }
		int DocumentTypeId { get; }
		string WriterName { get; }
		string CreatorName { get; }
		int WriterId { get; }
		int CreatorId { get; }
		string Path { get; }
		DateTime CreateDate { get; }
		DateTime UpdateDate { get; }
		Guid Version { get; }
		int Level { get; }
		string Url { get; }

        /// <summary>
        /// Gets a value indicating whether the content is a content (aka a document) or a media.
        /// </summary>
		PublishedItemType ItemType { get; }

        /// <summary>
        /// Gets a value indicating whether the content is draft.
        /// </summary>
        /// <remarks>A content is draft when it is the unpublished version of a content, which may
        /// have a published version, or not.</remarks>
        bool IsDraft { get; }

        /// <summary>
        /// Gets the index of the published content within its current owning content set.
        /// </summary>
        /// <returns>The index of the published content within its current owning content set.</returns>
	    int GetIndex();

        #endregion

        #region Tree

        /// <summary>
        /// Gets the parent of the content.
        /// </summary>
        /// <remarks>The parent of root content is <c>null</c>.</remarks>
		IPublishedContent Parent { get; }

        /// <summary>
        /// Gets the children of the content.
        /// </summary>
        /// <remarks>Children are sorted by their sortOrder.</remarks>
		IEnumerable<IPublishedContent> Children { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the properties of the content.
        /// </summary>
        /// <remarks>
        /// <para>Contains one <c>IPublishedProperty</c> for each property defined for the content type, including
        /// inherited properties. Some properties may have no value.</para>
        /// <para>The properties collection of an IPublishedContent instance should be read-only ie it is illegal
        /// to add properties to the collection.</para>
        /// </remarks>
		ICollection<IPublishedProperty> Properties { get; }

        /// <summary>
        /// Gets a property identified by its alias.
        /// </summary>
        /// <param name="alias">The property alias.</param>
        /// <returns>The property identified by the alias.</returns>
        /// <remarks>
        /// <para>If the content type has no property with that alias, including inherited properties, returns <c>null</c>,</para>
        /// <para>otherwise return a property -- that may have no value (ie <c>HasValue</c> is <c>false</c>).</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        IPublishedProperty GetProperty(string alias);

        /// <summary>
        /// Gets a property identified by its alias.
        /// </summary>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to navigate the tree upwards until a property with a value is found.</param>
        /// <returns>The property identified by the alias.</returns>
        /// <remarks>
        /// <para>Navigate the tree upwards and look for a property with that alias and with a value (ie <c>HasValue</c> is <c>true</c>).
        /// If found, return the property. If no property with that alias is found, having a value or not, return <c>null</c>. Otherwise
        /// return the first property that was found with the alias but had no value (ie <c>HasValue</c> is <c>false</c>).</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        IPublishedProperty GetProperty(string alias, bool recurse);

        /// <summary>
		/// Gets the value of a property identified by its alias.
		/// </summary>
		/// <param name="alias">The property alias.</param>
		/// <returns>The value of the property identified by the alias.</returns>
        /// <remarks>
        /// <para>If <c>GetProperty(alias)</c> is <c>null</c> then returns <c>null</c> else return <c>GetProperty(alias).Value</c>.</para>
        /// <para>So if the property has no value, returns the default value for that property type.</para>
        /// <para>This one is defined here really because we cannot define index extension methods, but all it should do is:
        /// <code>var p = GetProperty(alias); return p == null ? null : p.Value;</code> and nothing else.</para>
        /// <para>The recursive syntax (eg "_title") is _not_ supported here.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
		object this[string alias] { get; } // todo - should obsolete this[alias] (when?)

        #endregion
    }
}