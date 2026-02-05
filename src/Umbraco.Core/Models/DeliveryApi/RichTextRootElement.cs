namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents the root element of rich text content in the Delivery API.
/// </summary>
public sealed class RichTextRootElement : IRichTextElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextRootElement" /> class.
    /// </summary>
    /// <param name="attributes">The HTML attributes of the root element.</param>
    /// <param name="elements">The child elements of the root element.</param>
    /// <param name="blocks">The block items embedded in the rich text content.</param>
    public RichTextRootElement(Dictionary<string, object> attributes, IEnumerable<IRichTextElement> elements, IEnumerable<ApiBlockItem> blocks)
    {
        Attributes = attributes;
        Elements = elements;
        Blocks = blocks;
    }

    /// <inheritdoc />
    public string Tag => "#root";

    /// <summary>
    ///     Gets the HTML attributes of the root element.
    /// </summary>
    public Dictionary<string, object> Attributes { get; }

    /// <summary>
    ///     Gets the child elements of the root element.
    /// </summary>
    public IEnumerable<IRichTextElement> Elements { get; }

    /// <summary>
    ///     Gets the block items embedded in the rich text content.
    /// </summary>
    public IEnumerable<ApiBlockItem> Blocks { get; }
}
