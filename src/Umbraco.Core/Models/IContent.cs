using System;

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
    }
}