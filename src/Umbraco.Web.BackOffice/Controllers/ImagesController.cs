using System.Web;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     A controller used to return images for media
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class ImagesController : UmbracoAuthorizedApiController
{
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly MediaFileManager _mediaFileManager;

    public ImagesController(
        MediaFileManager mediaFileManager,
        IImageUrlGenerator imageUrlGenerator)
    {
        _mediaFileManager = mediaFileManager;
        _imageUrlGenerator = imageUrlGenerator;
    }

    /// <summary>
    ///     Gets the big thumbnail image for the original image path
    /// </summary>
    /// <param name="originalImagePath"></param>
    /// <returns></returns>
    /// <remarks>
    ///     If there is no original image is found then this will return not found.
    /// </remarks>
    public IActionResult GetBigThumbnail(string originalImagePath) =>
        string.IsNullOrWhiteSpace(originalImagePath)
            ? Ok()
            : GetResized(originalImagePath, 500);

    /// <summary>
    ///     Gets a resized image for the image at the given path
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    /// <remarks>
    ///     If there is no media, image property or image file is found then this will return not found.
    /// </remarks>
    public IActionResult GetResized(string imagePath, int width)
    {
        // We have to use HttpUtility to encode the path here, for non-ASCII characters
        // We cannot use the WebUtility, as we only want to encode the path, and not the entire string
        var encodedImagePath = HttpUtility.UrlPathEncode(imagePath);


        var ext = Path.GetExtension(encodedImagePath);

        // check if imagePath is local to prevent open redirect
        if (!Uri.IsWellFormedUriString(encodedImagePath, UriKind.Relative))
        {
            return Unauthorized();
        }

        // we need to check if it is an image by extension
        if (_imageUrlGenerator.IsSupportedImageFormat(ext) == false)
        {
            return NotFound();
        }

        // redirect to ImageProcessor thumbnail with rnd generated from last modified time of original media file
        DateTimeOffset? imageLastModified = null;
        try
        {
            imageLastModified = _mediaFileManager.FileSystem.GetLastModified(imagePath);
        }
        catch (Exception)
        {
            // if we get an exception here it's probably because the image path being requested is an image that doesn't exist
            // in the local media file system. This can happen if someone is storing an absolute path to an image online, which
            // is perfectly legal but in that case the media file system isn't going to resolve it.
            // so ignore and we won't set a last modified date.
        }

        var rnd = imageLastModified.HasValue ? $"&rnd={imageLastModified:yyyyMMddHHmmss}" : null;
        var imageUrl = _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(encodedImagePath)
        {
            Width = width,
            ImageCropMode = ImageCropMode.Max,
            CacheBusterValue = rnd
        });
        if (Url.IsLocalUrl(imageUrl))
        {
            return new LocalRedirectResult(imageUrl, false);
        }

        return Unauthorized();
    }

    /// <summary>
    ///     Gets a processed image for the image at the given path
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="focalPointLeft"></param>
    /// <param name="focalPointTop"></param>
    /// <param name="mode"></param>
    /// <param name="cacheBusterValue"></param>
    /// <param name="cropX1"></param>
    /// <param name="cropX2"></param>
    /// <param name="cropY1"></param>
    /// <param name="cropY2"></param>
    /// <returns></returns>
    /// <remarks>
    ///     If there is no media, image property or image file is found then this will return not found.
    /// </remarks>
    public string? GetProcessedImageUrl(
        string imagePath,
        int? width = null,
        int? height = null,
        decimal? focalPointLeft = null,
        decimal? focalPointTop = null,
        ImageCropMode mode = ImageCropMode.Max,
        string cacheBusterValue = "",
        decimal? cropX1 = null,
        decimal? cropX2 = null,
        decimal? cropY1 = null,
        decimal? cropY2 = null)
    {
        var options = new ImageUrlGenerationOptions(imagePath)
        {
            Width = width,
            Height = height,
            ImageCropMode = mode,
            CacheBusterValue = cacheBusterValue
        };

        if (focalPointLeft.HasValue && focalPointTop.HasValue)
        {
            options.FocalPoint =
                new ImageUrlGenerationOptions.FocalPointPosition(focalPointLeft.Value, focalPointTop.Value);
        }
        else if (cropX1.HasValue && cropX2.HasValue && cropY1.HasValue && cropY2.HasValue)
        {
            options.Crop =
                new ImageUrlGenerationOptions.CropCoordinates(cropX1.Value, cropY1.Value, cropX2.Value, cropY2.Value);
        }

        return _imageUrlGenerator.GetImageUrl(options);
    }
}
