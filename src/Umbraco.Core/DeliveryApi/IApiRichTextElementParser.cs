using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a parser that processes rich text content into structured elements for the Delivery API.
/// </summary>
public interface IApiRichTextElementParser
{
    /// <summary>
    ///     Parses HTML markup and optional block model into a rich text element.
    /// </summary>
    /// <param name="html">The HTML markup to parse.</param>
    /// <param name="richTextBlockModel">The optional rich text block model containing embedded blocks.</param>
    /// <returns>A rich text element, or <c>null</c> if parsing fails.</returns>
    IRichTextElement? Parse(string html, RichTextBlockModel? richTextBlockModel);
}
