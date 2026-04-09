namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a rich text model in the Delivery API.
/// </summary>
public class RichTextModel
{
    /// <summary>
    ///     Gets or sets the HTML markup of the rich text content.
    /// </summary>
    public required string Markup { get; set; }

    /// <summary>
    ///     Gets or sets the block items embedded in the rich text content.
    /// </summary>
    public required IEnumerable<ApiBlockItem> Blocks { get; set; }

    /// <summary>
    ///     Creates an empty <see cref="RichTextModel" /> instance.
    /// </summary>
    /// <returns>A new <see cref="RichTextModel" /> instance with empty markup and no blocks.</returns>
    public static RichTextModel Empty() => new() { Markup = string.Empty, Blocks = Array.Empty<ApiBlockItem>() };
}
