using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IImagingCrops
    {
        bool SaveFiles { get; }

        IEnumerable<IImagingCrop> Crops { get; }
    }
}