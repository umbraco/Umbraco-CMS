using System;
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
        /// Gets or sets the content schedule
        /// </summary>
        ContentScheduleCollection ContentSchedule { get; set; }

        /// <summary>
        /// Gets or sets the template id used to render the content.
        /// </summary>
        int? TemplateId { get; set; }

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
        /// Gets the template id used to render the published version of the content.
        /// </summary>
        /// <remarks>When editing the content, the template can change, but this will not until the content is published.</remarks>
        int? PublishTemplateId { get; }

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
        /// Gets the content type of this content.
        /// </summary>
        ISimpleContentType ContentType { get; }

        /// <summary>
        /// Gets a value indicating whether a culture is published.
        /// </summary>
        /// <remarks>
        /// <para>A culture becomes published whenever values for this culture are published,
        /// and the content published name for this culture is non-null. It becomes non-published
        /// whenever values for this culture are unpublished.</para>
        /// <para>A culture becomes published as soon as PublishCulture has been invoked,
        /// even though the document might now have been saved yet (and can have no identity).</para>
        /// <para>Does not support the '*' wildcard (returns false).</para>
        /// </remarks>
        bool IsCulturePublished(string culture);

        /// <summary>
        /// Gets a value indicating whether a culture was published.
        /// </summary>
        /// <remarks>
        /// <para>Mirrors <see cref="IsCulturePublished"/> whenever the content item is saved.</para>
        /// </remarks>
        bool WasCulturePublished(string culture);

        /// <summary>
        /// Gets the date a culture was published.
        /// </summary>
        DateTime? GetPublishDate(string culture);

        /// <summary>
        /// Gets a value indicated whether a given culture is edited.
        /// </summary>
        /// <remarks>
        /// <para>A culture is edited when it is available, and not published or published but
        /// with changes.</para>
        /// <para>A culture can be edited even though the document might now have been saved yet (and can have no identity).</para>
        /// <para>Does not support the '*' wildcard (returns false).</para>
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
        /// Gets the published culture infos of the content.
        /// </summary>
        /// <remarks>
        /// <para>Because a dictionary key cannot be <c>null</c> this cannot get the invariant
        /// name, which must be get via the <see cref="PublishName"/> property.</para>
        /// </remarks>
        IReadOnlyDictionary<string, ContentCultureInfos> PublishCultureInfos { get; }

        /// <summary>
        /// Gets the published cultures.
        /// </summary>
        IEnumerable<string> PublishedCultures { get; }

        /// <summary>
        /// Gets the edited cultures.
        /// </summary>
        IEnumerable<string> EditedCultures { get; }

        // TODO: these two should move to some kind of service

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
        /// Registers a culture to be published.
        /// </summary>
        /// <returns>A value indicating whether the culture can be published.</returns>
        /// <remarks>
        /// <para>Fails if properties don't pass variant validation rules.</para>
        /// <para>Publishing must be finalized via the content service SavePublishing method.</para>
        /// </remarks>
        bool PublishCulture(string culture = "*");

        /// <summary>
        /// Registers a culture to be unpublished.
        /// </summary>
        /// <remarks>
        /// <para>Unpublishing must be finalized via the content service SavePublishing method.</para>
        /// </remarks>
        void UnpublishCulture(string culture = "*");

        /// <summary>
        /// Determines whether a culture is being published, during a Publishing event.
        /// </summary>
        /// <remarks>Outside of a Publishing event handler, the returned value is unspecified.</remarks>
        bool IsPublishingCulture(string culture);

        /// <summary>
        /// Determines whether a culture is being unpublished, during a Publishing event.
        /// </summary>
        /// <remarks>Outside of a Publishing event handler, the returned value is unspecified.</remarks>
        bool IsUnpublishingCulture(string culture);

        /// <summary>
        /// Determines whether a culture has been published, during a Published event.
        /// </summary>
        /// <remarks>Outside of a Published event handler, the returned value is unspecified.</remarks>
        bool HasPublishedCulture(string culture);

        /// <summary>
        /// Determines whether a culture has been unpublished, during a Published event.
        /// </summary>
        /// <remarks>Outside of a Published event handler, the returned value is unspecified.</remarks>
        bool HasUnpublishedCulture(string culture);
    }
}
