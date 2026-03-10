namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a generic HTML element within rich text content in the Delivery API.
/// </summary>
public sealed class RichTextGenericElement : IRichTextElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextGenericElement" /> class.
    /// </summary>
    /// <param name="tag">The tag name of the element (e.g., "p", "h1", "strong").</param>
    /// <param name="attributes">The HTML attributes of the element.</param>
    /// <param name="elements">The child elements of the element.</param>
    public RichTextGenericElement(string tag, Dictionary<string, object> attributes, IEnumerable<IRichTextElement> elements)
    {
        Tag = tag;
        Attributes = attributes;
        Elements = elements;
    }

    /// <inheritdoc />
    public string Tag { get; }

    /// <summary>
    ///     Gets the HTML attributes of the element.
    /// </summary>
    public Dictionary<string, object> Attributes { get; }

    /// <summary>
    ///     Gets the child elements of the element.
    /// </summary>
    public IEnumerable<IRichTextElement> Elements { get; }
}
