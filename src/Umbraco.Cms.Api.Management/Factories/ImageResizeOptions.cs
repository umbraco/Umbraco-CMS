using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Options for resizing images when generating thumbnail URLs.
/// </summary>
/// <param name="Height">The height of the resized image in pixels. Default is 200.</param>
/// <param name="Width">The width of the resized image in pixels. Default is 200.</param>
/// <param name="Mode">The crop mode to use when resizing. Default is null (no specific mode).</param>
/// <param name="Format">The output format for the image (e.g., "webp", "png", "jpg"). Default is null (original format).</param>
public record ImageResizeOptions(
    int Height = 200,
    int Width = 200,
    ImageCropMode? Mode = null,
    string? Format = null);
