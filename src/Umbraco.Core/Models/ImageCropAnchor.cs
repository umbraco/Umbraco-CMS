namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines the anchor point for image cropping operations.
/// </summary>
public enum ImageCropAnchor
{
    /// <summary>
    ///     Anchor to the center of the image.
    /// </summary>
    Center,

    /// <summary>
    ///     Anchor to the top of the image.
    /// </summary>
    Top,

    /// <summary>
    ///     Anchor to the right of the image.
    /// </summary>
    Right,

    /// <summary>
    ///     Anchor to the bottom of the image.
    /// </summary>
    Bottom,

    /// <summary>
    ///     Anchor to the left of the image.
    /// </summary>
    Left,

    /// <summary>
    ///     Anchor to the top-left corner of the image.
    /// </summary>
    TopLeft,

    /// <summary>
    ///     Anchor to the top-right corner of the image.
    /// </summary>
    TopRight,

    /// <summary>
    ///     Anchor to the bottom-left corner of the image.
    /// </summary>
    BottomLeft,

    /// <summary>
    ///     Anchor to the bottom-right corner of the image.
    /// </summary>
    BottomRight,
}
