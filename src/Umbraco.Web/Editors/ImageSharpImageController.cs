using System.Web.Mvc;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    public class ImageSharpImageController : SurfaceController
    {
        private readonly IImageSharpImageService _imageSharpImageService;
        public ImageSharpImageController(IImageSharpImageService imageSharpImageService)
        {
            _imageSharpImageService = imageSharpImageService;
        }
        public ActionResult ManipulateImage(string imagePath, int? width, int? height)
        {
            var imageModel = _imageSharpImageService.GetImage(imagePath, width, height);

            return new FileContentResult(imageModel.Data, imageModel.MimeType);
        }
    }
}
