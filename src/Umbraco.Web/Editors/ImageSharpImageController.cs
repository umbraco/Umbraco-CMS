using System.Web.Mvc;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    public class ImageSharpImageController : PluginController
    {
        private readonly IImageSharpImageService _imageSharpImageSertvice;
        public ImageSharpImageController(IImageSharpImageService imageSharpImageSertvice)
        {
            _imageSharpImageSertvice = imageSharpImageSertvice;
        }
        public ActionResult ManipulateImage(string imagePath, int? width, int? height)
        {
            var imageModel = _imageSharpImageSertvice.GetImage(imagePath, width, height);

            return new FileContentResult(imageModel.Data, imageModel.MimeType);
        }
    }
}
