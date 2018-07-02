﻿using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a document.
    /// </summary>
    /// <remarks>
    /// <para>A document can be published, rendered by a template.</para>
    /// </remarks>
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

        /// <summary>
        /// Gets a value indicating whether a given culture is published.
        /// </summary>
        /// <remarks>
        /// <para>A culture becomes published whenever values for this culture are published,
        /// and the content published name for this culture is non-null. It becomes non-published
        /// whenever values for this culture are unpublished.</para>
        /// </remarks>
        bool IsCulturePublished(string culture);

        /// <summary>
        /// Gets the date a culture was published.
        /// </summary>
        DateTime GetCulturePublishDate(string culture);

        /// <summary>
        /// Gets a value indicated whether a given culture is edited.
        /// </summary>
        /// <remarks>
        /// <para>A culture is edited when it is not published, or when it is published but
        /// it has changes.</para>
        /// </remarks>
        bool IsCultureEdited(string culture);

        /// <summary>
        /// Gets the name of the published version of the content for a given culture.
        /// </summary>
        /// <remarks>
        /// <para>When editing the content, the name can change, but this will not until the content is published.</para>
        /// <para>When <paramref name="culture"/> is <c>null</c>, gets the invariant
        /// language, which is the value of the <see cref="PublishName"/> property.</para>
        /// </remarks>
        string GetPublishName(string culture);

        /// <summary>
        /// Gets the published names of the content.
        /// </summary>
        /// <remarks>
        /// <para>Because a dictionary key cannot be <c>null</c> this cannot get the invariant
        /// name, which must be get via the <see cref="PublishName"/> property.</para>
        /// </remarks>
        IReadOnlyDictionary<string, string> PublishCultureNames { get; }

        /// <summary>
        /// Gets the available cultures.
        /// </summary>
        IEnumerable<string> AvailableCultures { get; }

        /// <summary>
        /// Gets the published cultures.
        /// </summary>
        IEnumerable<string> PublishedCultures { get; }

        /// <summary>
        /// Gets the edited cultures.
        /// </summary>
        IEnumerable<string> EditedCultures { get; }

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
        /// <para>Values are not published if they are not valid.</para>
        /// </remarks>
        //fixme return an Attempt with some error results if it doesn't work
        //fixme - needs API review as this is not used apart from in tests
        //bool TryPublishAllValues();

        /// <summary>
        /// Publishes values.
        /// </summary>
        /// <returns>A value indicating whether the values could be published.</returns>
        /// <remarks>
        /// <para>The document must then be published via the content service.</para>
        /// <para>Values are not published if they are not valid.</para>
        /// </remarks>
        //fixme return an Attempt with some error results if it doesn't work
        bool TryPublishValues(string culture = null, string segment = null);

        /// <summary>
        /// Publishes the culture/any values.
        /// </summary>
        /// <returns>A value indicating whether the values could be published.</returns>
        /// <remarks>
        /// <para>The document must then be published via the content service.</para>
        /// <para>Values are not published if they are not valie.</para>
        /// </remarks>
        //fixme - needs API review as this is not used apart from in tests
        //bool PublishCultureValues(string culture = null);

        /// <summary>
        /// Clears all published values.
        /// </summary>
        void ClearAllPublishedValues();

        /// <summary>
        /// Clears published values.
        /// </summary>
        void ClearPublishedValues(string culture = null, string segment = null);

        /// <summary>
        /// Clears the culture/any published values.
        /// </summary>
        void ClearCulturePublishedValues(string culture = null);

        /// <summary>
        /// Copies values from another document.
        /// </summary>
        void CopyAllValues(IContent other);

        /// <summary>
        /// Copies values from another document.
        /// </summary>
        void CopyValues(IContent other, string culture = null, string segment = null);

        /// <summary>
        /// Copies culture/any values from another document.
        /// </summary>
        void CopyCultureValues(IContent other, string culture = null);
    }
}
