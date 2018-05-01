using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{

    /// <inheritdoc />
    /// <summary>
    /// Represents a cached content.
    /// </summary>
    public interface IPublishedContent : IPublishedElement
    {
        #region Content

        // fixme - all these are colliding with models => ?
        // or could we force them to be 'new' in models?

        int Id { get; }
        int TemplateId { get; }
        int SortOrder { get; }
        string Name { get; }
        string UrlName { get; } // fixme rename
        string DocumentTypeAlias { get; } // fixme obsolete
        int DocumentTypeId { get; } // fixme obsolete
        string WriterName { get; }
        string CreatorName { get; }
        int WriterId { get; }
        int CreatorId { get; }
        string Path { get; }
        DateTime CreateDate { get; }
        DateTime UpdateDate { get; }
        int Level { get; }
        string Url { get; }

        IReadOnlyDictionary<string, PublishedCultureName> CultureNames { get; }

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

        #endregion
    }
}
