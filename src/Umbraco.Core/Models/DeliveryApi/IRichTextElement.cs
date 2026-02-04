using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents an element within rich text content in the Delivery API.
/// </summary>
[JsonDerivedType(typeof(RichTextRootElement))]
[JsonDerivedType(typeof(RichTextGenericElement))]
[JsonDerivedType(typeof(RichTextTextElement))]
public interface IRichTextElement
{
    /// <summary>
    ///     Gets the tag name of the element (e.g., "p", "h1", "#text", "#root").
    /// </summary>
    string Tag { get; }
}
