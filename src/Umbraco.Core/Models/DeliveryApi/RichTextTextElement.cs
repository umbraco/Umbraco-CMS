namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a text node within rich text content in the Delivery API.
/// </summary>
public sealed class RichTextTextElement : IRichTextElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextTextElement" /> class.
    /// </summary>
    /// <param name="text">The text content of the element.</param>
    public RichTextTextElement(string text)
        => Text = text;

    /// <summary>
    ///     Gets the text content of the element.
    /// </summary>
    public string Text { get; }

    /// <inheritdoc />
    public string Tag => "#text";
}
