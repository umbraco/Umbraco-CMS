using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IImagingCrop
    {
        string MediaTypeAlias { get; }

        string FocalPointProperty { get; }

        IEnumerable<IImagingCropSize> CropSizes { get; }
    }
}