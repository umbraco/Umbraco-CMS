using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a strongly-typed model for the current Umbraco view.
/// </summary>
/// <typeparam name="TContent">The type of the published content, which must implement <see cref="IPublishedContent" />.</typeparam>
/// <remarks>
///     This generic class extends <see cref="ContentModel" /> to provide type-safe access to the content
///     in Umbraco views and controllers.
/// </remarks>
public class ContentModel<TContent> : ContentModel
    where TContent : IPublishedContent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentModel{TContent}" /> class with a content.
    /// </summary>
    public ContentModel(TContent content)
        : base(content) => Content = content;

    /// <summary>
    ///     Gets the content.
    /// </summary>
    public new TContent Content { get; }
}
