using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models;

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
