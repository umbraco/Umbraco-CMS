using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides strongly typed published content models services.
/// </summary>
public static class PublishedContentExtensionsForModels
{
    /// <summary>
    ///     Creates a strongly typed published content model for an internal published content.
    /// </summary>
    /// <param name="content">The internal published content.</param>
    /// <param name="publishedModelFactory">The published model factory</param>
    /// <returns>The strongly typed published content model.</returns>
    public static IPublishedContent? CreateModel(
        this IPublishedContent? content,
        IPublishedModelFactory? publishedModelFactory)
        => CreateModel<IPublishedContent>(content, publishedModelFactory);

    /// <summary>
    ///     Creates a strongly typed published content model for an internal published element.
    /// </summary>
    /// <param name="element">The internal published element.</param>
    /// <param name="publishedModelFactory">The published model factory</param>
    /// <returns>The strongly typed published element model.</returns>
    public static IPublishedElement? CreateModel(
        this IPublishedElement? element,
        IPublishedModelFactory? publishedModelFactory)
        => CreateModel<IPublishedElement>(element, publishedModelFactory);

    private static T? CreateModel<T>(
        IPublishedElement? content,
        IPublishedModelFactory? publishedModelFactory)
        where T : IPublishedElement
    {
        if (publishedModelFactory == null)
        {
            throw new ArgumentNullException(nameof(publishedModelFactory));
        }

        if (content == null)
        {
            return default;
        }

        // get model
        // if factory returns nothing, throw
        IPublishedElement model = publishedModelFactory.CreateModel(content);
        if (model == null)
        {
            throw new InvalidOperationException("Factory returned null.");
        }

        // if factory returns a different type, throw
        if (!(model is T publishedContent))
        {
            throw new InvalidOperationException(
                $"Factory returned model of type {model.GetType().FullName} which does not implement {typeof(T).Name}.");
        }

        return publishedContent;
    }
}
