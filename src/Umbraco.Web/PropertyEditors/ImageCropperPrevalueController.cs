using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.PropertyEditors
{
    
    public class ImageCropperPreValueController : UmbracoAuthorizedJsonController
    {
        //Returns the collection of allowed crops on a given media alias type
        public Models.ImageCropDataSet GetConfiguration(string mediaTypeAlias)
        {
            return ImageCropperPropertyEditorHelper.GetConfigurationForType(mediaTypeAlias);
        }

        //Returns a specific crop on a media type
        public Models.ImageCropData GetCrop(string mediaTypeAlias, string cropAlias)
        {
            return ImageCropperPropertyEditorHelper.GetCrop(mediaTypeAlias, cropAlias);
        }
    }
}
