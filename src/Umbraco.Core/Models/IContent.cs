using System;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a Content object
    /// </summary>
    public interface IContent : IContentBase
    {
        /// <summary>
        /// Alias of the template used by the Content
        /// This is used to override the default one from the ContentType
        /// </summary>
        string Template { get; set; }

        /// <summary>
        /// Boolean indicating whether the Content is Published or not
        /// </summary>
        bool Published { get; }

        /// <summary>
        /// Language of the data contained within the Content object
        /// </summary>
        //string Language { get; set; }

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
        /// <param name="isPublished">Boolean indicating whether content is published (true) or unpublished (false)</param>
        void ChangePublishedState(bool isPublished);

        /// <summary>
        /// Changes the Trashed state of the content object
        /// </summary>
        /// <param name="isTrashed">Boolean indicating whether content is trashed (true) or not trashed (false)</param>
        /// <param name="parentId"> </param>
        void ChangeTrashedState(bool isTrashed, int parentId = -1);
    }
}