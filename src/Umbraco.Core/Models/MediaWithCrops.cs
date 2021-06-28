using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a media item with local crops.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Models.PublishedContent.PublishedContentWrapped" />
    public class MediaWithCrops : PublishedContentWrapped
    {
        /// <summary>
        /// Gets the media item.
        /// </summary>
        /// <value>
        /// The media item.
        /// </value>
        [Obsolete("This instance now implements IPublishedContent by wrapping the media item, use the extension methods directly on MediaWithCrops or use the Content property to get the media item instead.")]
        public IPublishedContent MediaItem => Content;

        /// <summary>
        /// Gets the content/media item.
        /// </summary>
        /// <value>
        /// The content/media item.
        /// </value>
        public IPublishedContent Content => Unwrap();

        /// <summary>
        /// Gets the local crops.
        /// </summary>
        /// <value>
        /// The local crops.
        /// </value>
        public ImageCropperValue LocalCrops { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaWithCrops" /> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="localCrops">The local crops.</param>
        public MediaWithCrops(IPublishedContent content, ImageCropperValue localCrops)
            : base(content)
        {
            LocalCrops = localCrops;
        }
    }

    /// <summary>
    /// Represents a media item with local crops.
    /// </summary>
    /// <typeparam name="T">The type of the media item.</typeparam>
    /// <seealso cref="Umbraco.Core.Models.PublishedContent.PublishedContentWrapped" />
    public class MediaWithCrops<T> : MediaWithCrops
        where T : IPublishedContent
    {
        /// <summary>
        /// Gets the media item.
        /// </summary>
        /// <value>
        /// The media item.
        /// </value>
        public new T Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaWithCrops{T}" /> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="localCrops">The local crops.</param>
        public MediaWithCrops(T content, ImageCropperValue localCrops)
            : base(content, localCrops)
        {
            Content = content;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="MediaWithCrops{T}" /> to <see cref="T" />.
        /// </summary>
        /// <param name="mediaWithCrops">The media with crops.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator T(MediaWithCrops<T> mediaWithCrops) => mediaWithCrops.Content;
    }
}
