using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for getting URLs for images with parameters
/// </summary>
/// <remarks>
///     <para>
///         This controller allows for retrieving URLs for processed images, such as resized, cropped,
///         or otherwise altered.  These can be different based on the IImageUrlGenerator
///         implementation in use, and so the BackOffice could should not rely on hard-coded string
///         building to generate correct URLs
///     </para>
/// </remarks>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class ImageUrlGeneratorController : UmbracoAuthorizedJsonController
{
    private readonly IImageUrlGenerator _imageUrlGenerator;

    public ImageUrlGeneratorController(IImageUrlGenerator imageUrlGenerator) => _imageUrlGenerator = imageUrlGenerator;

    public string? GetCropUrl(string mediaPath, int? width = null, int? height = null, ImageCropMode? imageCropMode = null) => _imageUrlGenerator.GetImageUrl(
        new ImageUrlGenerationOptions(mediaPath) { Width = width, Height = height, ImageCropMode = imageCropMode });
}
