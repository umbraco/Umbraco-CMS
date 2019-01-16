using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{

    /// <inheritdoc />
    /// <summary>
    /// Represents a published content item.
    /// </summary>
    /// <remarks>
    /// <para>Can be a published document, media or member.</para>
    /// </remarks>
    public interface IPublishedContent : IPublishedElement
    {
        #region Content

        // todo - IPublishedContent properties colliding with models
        // we need to find a way to remove as much clutter as possible from IPublishedContent,
        // since this is preventing someone from creating a property named 'Path' and have it
        // in a model, for instance. we could move them all under one unique property eg
        // Infos, so we would do .Infos.SortOrder - just an idea - not going to do it in v8

        /// <summary>
        /// Gets the unique identifier of the content item.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the name of the content item.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is contextual. When the content type is multi-lingual,
        /// this is the name for the 'current' culture. Otherwise, it is the invariant name.</para>
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the url segment of the content item.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is contextual. When the content type is multi-lingual,
        /// this is the name for the 'current' culture. Otherwise, it is the invariant url segment.</para>
        /// </remarks>
        string UrlSegment { get; }

        /// <summary>
        /// Gets the sort order of the content item.
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// Gets the tree level of the content item.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Gets the tree path of the content item.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the identifier of the template to use to render the content item.
        /// </summary>
        int? TemplateId { get; }

        /// <summary>
        /// Gets the identifier of the user who created the content item.
        /// </summary>
        int CreatorId { get; }

        /// <summary>
        /// Gets the name of the user who created the content item.
        /// </summary>
        string CreatorName { get; }

        /// <summary>
        /// Gets the date the content item was created.
        /// </summary>
        DateTime CreateDate { get; }

        /// <summary>
        /// Gets the identifier of the user who last updated the content item.
        /// </summary>
        int WriterId { get; }

        /// <summary>
        /// Gets the name of the user who last updated the content item.
        /// </summary>
        string WriterName { get; }

        /// <summary>
        /// Gets the date the content item was last updated.
        /// </summary>
        /// <remarks>
        /// <para>For published content items, this is also the date the item was published.</para>
        /// <para>This date is always global to the content item, see GetCulture().Date for the
        /// date each culture was published.</para>
        /// </remarks>
        DateTime UpdateDate { get; }

        /// <summary>
        /// Gets the url of the content item.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the url for the
        /// 'current' culture. Otherwise, it is the invariant url.</para>
        /// </remarks>
        string Url { get; }

        /// <summary>
        /// Gets the url of the content item.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the url for the
        /// specified culture. Otherwise, it is the invariant url.</para>
        /// </remarks>
        string GetUrl(string culture = null);

        /// <summary>
        /// Gets culture infos for a culture.
        /// </summary>
        PublishedCultureInfo GetCulture(string culture = null);

        /// <summary>
        /// Gets culture infos.
        /// </summary>
        /// <remarks>
        /// <para>Contains only those culture that are available. For a published content, these are
        /// the cultures that are published. For a draft content, those that are 'available' ie
        /// have a non-empty content name.</para>
        /// </remarks>
        IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

        /// <summary>
        /// Gets the type of the content item (document, media...).
        /// </summary>
        PublishedItemType ItemType { get; }

        /// <summary>
        /// Gets a value indicating whether the content is draft.
        /// </summary>
        /// <remarks>
        /// <para>A content is draft when it is the unpublished version of a content, which may
        /// have a published version, or not.</para>
        /// <para>When retrieving documents from cache in non-preview mode, IsDraft is always false,
        /// as only published documents are returned. When retrieving in preview mode, IsDraft can
        /// either be true (document is not published, or has been edited, and what is returned
        /// is the edited version) or false (document is published, and has not been edited, and
        /// what is returned is the published version).</para>
        /// </remarks>
        bool IsDraft(string culture = null);

        // fixme - consider having an IsPublished flag too
        // so that when IsDraft is true, we can check whether there is a published version?

        #endregion

        #region Tree

        /// <summary>
        /// Gets the parent of the content item.
        /// </summary>
        /// <remarks>The parent of root content is <c>null</c>.</remarks>
        IPublishedContent Parent { get; }

        /// <summary>
        /// Gets the children of the content item.
        /// </summary>
        /// <remarks>Children are sorted by their sortOrder.</remarks>
        IEnumerable<IPublishedContent> Children { get; }

        #endregion
    }
}
