using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines a service for asynchronously retrieving embeddable HTML markup for a specified resource using the oEmbed
/// protocol.
/// </summary>
public interface IOEmbedService
{
    /// <summary>
    /// Asynchronously retrieves the embeddable HTML markup for the specified resource.
    /// </summary>
    /// <remarks>The returned markup is suitable for embedding in web pages. The width and height parameters
    /// may be ignored by some providers depending on their capabilities.</remarks>
    /// <param name="url">The URI of the resource to retrieve markup for. Must be a valid, absolute URI.</param>
    /// <param name="width">The optional maximum width, in pixels, for the embedded content. If null, the default width is used.</param>
    /// <param name="height">The optional maximum height, in pixels, for the embedded content. If null, the default height is used.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation is canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains an Attempt with the HTML markup if
    /// successful, or an oEmbed operation status indicating the reason for failure.</returns>
    Task<Attempt<string, OEmbedOperationStatus>> GetMarkupAsync(Uri url, int? width, int? height, CancellationToken cancellationToken);
}
