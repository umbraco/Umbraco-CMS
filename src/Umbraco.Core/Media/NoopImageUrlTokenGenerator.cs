namespace Umbraco.Cms.Core.Media;

/// <summary>
/// Default <see cref="IImageUrlTokenGenerator"/> implementation that returns URLs unchanged.
/// </summary>
/// <remarks>
/// Registered as the fallback in Core so that consumers can inject <see cref="IImageUrlTokenGenerator"/>
/// unconditionally. The ImageSharp 3+ package replaces this registration with a real signer; ImageSharp 2
/// (which has no HMAC support) and any custom imaging package leave the no-op in place.
/// </remarks>
internal sealed class NoopImageUrlTokenGenerator : IImageUrlTokenGenerator
{
    /// <inheritdoc />
    public string RefreshSignature(string url) => url;
}
