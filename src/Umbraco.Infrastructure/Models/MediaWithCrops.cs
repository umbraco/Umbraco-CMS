using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a media item with local crops.
/// </summary>
/// <seealso cref="PublishedContentWrapped" />
public class MediaWithCrops : PublishedContentWrapped
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaWithCrops" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="localCrops">The local crops.</param>
    public MediaWithCrops(IPublishedContent content, IPublishedValueFallback publishedValueFallback, ImageCropperValue localCrops)
        : base(content, publishedValueFallback) =>
        LocalCrops = localCrops;

    /// <summary>
    ///     Gets the content/media item.
    /// </summary>
    /// <value>
    ///     The content/media item.
    /// </value>
    public IPublishedContent Content => Unwrap();

    /// <summary>
    ///     Gets the local crops.
    /// </summary>
    /// <value>
    ///     The local crops.
    /// </value>
    public ImageCropperValue LocalCrops { get; }
}

/// <summary>
///     Represents a media item with local crops.
/// </summary>
/// <typeparam name="T">The type of the media item.</typeparam>
/// <seealso cref="PublishedContentWrapped" />
public class MediaWithCrops<T> : MediaWithCrops
    where T : IPublishedContent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaWithCrops{T}" /> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    /// <param name="localCrops">The local crops.</param>
    public MediaWithCrops(T content, IPublishedValueFallback publishedValueFallback, ImageCropperValue localCrops)
        : base(content, publishedValueFallback, localCrops) =>
        Content = content;

    /// <summary>
    ///     Gets the media item.
    /// </summary>
    /// <value>
    ///     The media item.
    /// </value>
    public new T Content { get; }

    /// <summary>
    ///     Performs an implicit conversion from <see cref="MediaWithCrops{T}" /> to <see cref="T" />.
    /// </summary>
    /// <param name="mediaWithCrops">The media with crops.</param>
    /// <returns>
    ///     The result of the conversion.
    /// </returns>
    public static implicit operator T(MediaWithCrops<T> mediaWithCrops) => mediaWithCrops.Content;
}
