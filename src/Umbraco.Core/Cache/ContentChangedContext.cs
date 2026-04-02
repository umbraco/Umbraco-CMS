namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Describes a content change that triggered output cache eviction.
/// </summary>
public class ContentChangedContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentChangedContext"/> class.
    /// </summary>
    /// <param name="contentKey">The key of the content that changed.</param>
    /// <param name="publishedCultures">The cultures that were published, if applicable.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished, if applicable.</param>
    public ContentChangedContext(Guid contentKey, IReadOnlyCollection<string> publishedCultures, IReadOnlyCollection<string> unpublishedCultures)
    {
        ContentKey = contentKey;
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

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
