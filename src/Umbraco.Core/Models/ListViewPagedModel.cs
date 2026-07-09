using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a paged model for list view content items.
/// </summary>
/// <typeparam name="TContent">The type of content that implements <see cref="IContentBase" />.</typeparam>
public class ListViewPagedModel<TContent>
    where TContent : IContentBase
{
    /// <summary>
    ///     Gets or sets the paged collection of content items.
    /// </summary>
    public required PagedModel<TContent> Items { get; init; }

    /// <summary>
    ///     Gets or sets the list view configuration settings.
    /// </summary>
    public required ListViewConfiguration ListViewConfiguration { get; init; }
}
