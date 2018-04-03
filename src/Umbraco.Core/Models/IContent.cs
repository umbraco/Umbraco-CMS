using System;
using System.ComponentModel;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Content object
    /// </summary>
    public interface IContent : IContentBase
    {
        /// <summary>
        /// Gets or sets the template used to render the content.
        /// </summary>
        ITemplate Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the content is published.
        /// </summary>
        bool Published { get; }

        PublishedState PublishedState { get; }

        /// <summary>
        /// Gets a value indicating whether the content has been edited.
        /// </summary>
        bool Edited { get; }

        /// <summary>
        /// Gets the published version identifier.
        /// </summary>
        int PublishedVersionId { get; }

        /// <summary>
        /// Gets a value indicating whether the content item is a blueprint.
        /// </summary>
        bool Blueprint { get; }

        /// <summary>
        /// Gets the template used to render the published version of the content.
        /// </summary>
        /// <remarks>When editing the content, the template can change, but this will not until the content is published.</remarks>
        ITemplate PublishTemplate { get; }

        /// <summary>
        /// Gets the name of the published version of the content.
        /// </summary>
        /// <remarks>When editing the content, the name can change, but this will not until the content is published.</remarks>
        string PublishName { get; }

        /// <summary>
        /// Gets the identifier of the user who published the content.
        /// </summary>
        int? PublisherId { get; }

        /// <summary>
        /// Gets the date and time the content was published.
        /// </summary>
        DateTime? PublishDate { get; }
        
        /// <summary>
        /// Gets or sets the date and time the content item should be published.
        /// </summary>
        DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time the content should be unpublished.
        /// </summary>
        DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Gets the content type of this content.
        /// </summary>
        IContentType ContentType { get; }

        /// <summary>
        /// Gets the current status of the content.
        /// </summary>
        ContentStatus Status { get; }

        // fixme - these two should move to some kind of service

        /// <summary>
        /// Changes the <see cref="IContentType"/> for the current content object
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <remarks>Leaves PropertyTypes intact after change</remarks>
        void ChangeContentType(IContentType contentType);

        /// <summary>
        /// Changes the <see cref="IContentType"/> for the current content object and removes PropertyTypes,
        /// which are not part of the new ContentType.
        /// </summary>
        /// <param name="contentType">New ContentType for this content</param>
        /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
        void ChangeContentType(IContentType contentType, bool clearProperties);

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        IContent DeepCloneWithResetIdentities();

        /// <summary>
        /// Publishes all values.
        /// </summary>
        /// <returns>A value indicating whether the values could be published.</returns>
        /// <remarks>
        /// <para>The document must then be published via the content service.</para>
        /// <para>Values are not published if they are not valie.</para>
        /// </remarks>
        bool PublishAllValues();

        /// <summary>
        /// Publishes values.
        /// </summary>
        /// <returns>A value indicating whether the values could be published.</returns>
        /// <remarks>
        /// <para>The document must then be published via the content service.</para>
        /// <para>Values are not published if they are not valie.</para>
        /// </remarks>
        bool PublishValues(int? languageId = null, string segment = null);

        /// <summary>
        /// Publishes the culture/any values.
        /// </summary>
        /// <returns>A value indicating whether the values could be published.</returns>
        /// <remarks>
        /// <para>The document must then be published via the content service.</para>
        /// <para>Values are not published if they are not valie.</para>
        /// </remarks>
        bool PublishCultureValues(int? languageId = null);

        /// <summary>
        /// Clears all published values.
        /// </summary>
        void ClearAllPublishedValues();

        /// <summary>
        /// Clears published values.
        /// </summary>
        void ClearPublishedValues(int? languageId = null, string segment = null);

        /// <summary>
        /// Clears the culture/any published values.
        /// </summary>
        void ClearCulturePublishedValues(int? languageId = null);

        /// <summary>
        /// Copies values from another document.
        /// </summary>
        void CopyAllValues(IContent other);

        /// <summary>
        /// Copies values from another document.
        /// </summary>
        void CopyValues(IContent other, int? languageId = null, string segment = null);

        /// <summary>
        /// Copies culture/any values from another document.
        /// </summary>
        void CopyCultureValues(IContent other, int? languageId = null);
    }
}
