namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a parser that processes rich text HTML markup for the Delivery API.
/// </summary>
public interface IApiRichTextMarkupParser
{
    /// <summary>
    ///     Parses the specified HTML markup for Delivery API output.
    /// </summary>
    /// <param name="html">The HTML markup to parse.</param>
    /// <returns>The parsed HTML markup.</returns>
    string Parse(string html);
}
