using System;
using System.Diagnostics;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Content object
    /// </summary>    
    public interface IContent : IContentBase
    {
        /// <summary>
        /// Gets or sets the template used by the Content.
        /// This is used to override the default one from the ContentType.
        /// </summary>
        ITemplate Template { get; set; }

        /// <summary>
        /// Boolean indicating whether the Content is Published or not
        /// </summary>
        bool Published { get; }

        [Obsolete("This will be removed in future versions")]
        string Language { get; set; }

        /// <summary>
        /// Gets or Sets the date the Content should be released and thus be published
        /// </summary>
        DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or Sets the date the Content should expire and thus be unpublished
        /// </summary>
        DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Id of the user who wrote/updated the Content
        /// </summary>
        int WriterId { get; set; }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        IContentType ContentType { get; }

        /// <summary>
        /// Gets the current status of the Content
        /// </summary>
        ContentStatus Status { get; }

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
        /// Changes the Published state of the content object
        /// </summary>
        void ChangePublishedState(PublishedState state);

        /// <summary>
        /// Creates a deep clone of the current entity with its identity/alias and it's property identities reset
        /// </summary>
        /// <returns></returns>
        IContent DeepCloneWithResetIdentities();

        /// <summary>
        /// Gets a value indicating whether the content has a published version.
        /// </summary>
        bool HasPublishedVersion { get; }

        /// <summary>
        /// Gets the unique identifier of the published version, if any.
        /// </summary>
        Guid PublishedVersionGuid { get; }
    }
}