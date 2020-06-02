﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// The API controller used for getting URLs for images with parameters
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller allows for retrieving URLs for processed images, such as resized, cropped,
    /// or otherwise altered.  These can be different based on the IImageUrlGenerator
    /// implementation in use, and so the BackOffice could should not rely on hard-coded string
    /// building to generate correct URLs
    /// </para>
    /// </remarks>
    [PluginController("UmbracoApi")]
    public class ImageUrlGeneratorController : UmbracoAuthorizedJsonController
    {
        private readonly IImageUrlGenerator _imageUrlGenerator;

        public ImageUrlGeneratorController(IImageUrlGenerator imageUrlGenerator)
        {
            _imageUrlGenerator = imageUrlGenerator;
        }

        public string GetCropUrl(string mediaPath, int? width = null, int? height = null, ImageCropMode? imageCropMode = null, string animationProcessMode = null)
        {
            return _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(mediaPath)
            {
                Width = width,
                Height = height,
                ImageCropMode = imageCropMode,
                AnimationProcessMode = animationProcessMode
            });
        }
    }
}
