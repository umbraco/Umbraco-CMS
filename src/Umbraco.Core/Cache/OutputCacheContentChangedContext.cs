namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Describes a content change that triggered output cache eviction.
/// </summary>
public class OutputCacheContentChangedContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OutputCacheContentChangedContext"/> class.
    /// </summary>
    /// <param name="contentId">The integer ID of the content that changed.</param>
    /// <param name="contentKey">The key of the content that changed.</param>
    /// <param name="publishedCultures">The cultures that were published, if applicable.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, if applicable.</param>
    public OutputCacheContentChangedContext(int contentId, Guid contentKey, IReadOnlyCollection<string> publishedCultures, IReadOnlyCollection<string> unpublishedCultures)
    {
        ContentId = contentId;
        ContentKey = contentKey;
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    /// <summary>
    ///     Gets the integer ID of the content that changed.
    /// </summary>
    public int ContentId { get; }

    /// <summary>
    ///     Gets the key of the content that changed.
    /// </summary>
    public Guid ContentKey { get; }

    /// <summary>
    ///     Gets the cultures that were published, if applicable. Empty for invariant content.
    /// </summary>
    public IReadOnlyCollection<string> PublishedCultures { get; }

    /// <summary>
    ///     Gets the cultures that were unpublished, if applicable. Empty for invariant content.
    /// </summary>
    public IReadOnlyCollection<string> UnpublishedCultures { get; }
}
