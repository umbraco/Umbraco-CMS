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

        // TODO: IPublishedContent properties colliding with models
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
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        string Name(string culture = null);

        /// <summary>
        /// Gets the url segment of the content item.
        /// </summary>
        /// <param name="culture">The specific culture to get the url segment for. If null is used the current culture is used (Default is null).</param>
        string UrlSegment(string culture = null);

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
        /// <para>This date is always global to the content item, see CultureDate() for the
        /// date each culture was published.</para>
        /// </remarks>
        DateTime UpdateDate { get; }

        /// <summary>
        /// Gets the url of the content item.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the url for the
        /// specified culture. Otherwise, it is the invariant url.</para>
        /// </remarks>
        string Url(string culture = null, UrlMode mode = UrlMode.Auto);

        /// <summary>
        /// Gets the culture date of the content item.
        /// </summary>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        DateTime CultureDate(string culture = null);

        /// <summary>
        /// Gets all available cultures.
        /// </summary>
        /// <remarks>
        /// <para>Contains only those culture that are available. For a published content, these are
        /// the cultures that are published. For a draft content, those that are 'available' ie
        /// have a non-empty content name.</para>
        /// </remarks>
        IReadOnlyList<string> Cultures { get; }

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

        /// <summary>
        /// Gets a value indicating whether the content is published.
        /// </summary>
        /// <remarks>
        /// <para>A content is published when it has a published version.</para>
        /// <para>When retrieving documents from cache in non-preview mode, IsPublished is always
        /// true, as only published documents are returned. When retrieving in draft mode, IsPublished
        /// can either be true (document has a published version) or false (document has no
        /// published version).</para>
        /// <para>It is therefore possible for both IsDraft and IsPublished to be true at the same
        /// time, meaning that the content is the draft version, and a published version exists.</para>
        /// </remarks>
        bool IsPublished(string culture = null);

        #endregion

        #region Tree

        /// <summary>
        /// Gets the parent of the content item.
        /// </summary>
        /// <remarks>The parent of root content is <c>null</c>.</remarks>
        IPublishedContent Parent();

        /// <summary>
        /// Gets the children of the content item.
        /// </summary>
        /// <remarks>Children are sorted by their sortOrder.</remarks>
        IEnumerable<IPublishedContent> Children { get; }

        #endregion
    }
}
