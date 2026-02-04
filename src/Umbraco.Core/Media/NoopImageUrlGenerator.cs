using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Media;

/// <summary>
///     A no-operation implementation of <see cref="IImageUrlGenerator"/> that returns the original image URL without modifications.
/// </summary>
public sealed class NoopImageUrlGenerator : IImageUrlGenerator
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; } = Enumerable.Empty<string>();

    /// <inheritdoc />
    public string? GetImageUrl(ImageUrlGenerationOptions options) => options?.ImageUrl;
}
