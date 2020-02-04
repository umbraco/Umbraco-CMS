using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ActionResult ManipulateImage(int width, int height, string imageUri)
        {
            var imageModel = _imageSharpImageSertvice.GetImage(imageUri, width, height);

            return new FileContentResult(imageModel.Data, imageModel.MimeType);
        }
    }
}
