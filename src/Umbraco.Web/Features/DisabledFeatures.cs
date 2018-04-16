using Umbraco.Core.Collections;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents disabled features.
    /// </summary>
    internal class DisabledFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisabledFeatures"/> class.
        /// </summary>
        public DisabledFeatures()
        {
            Controllers = new TypeList<UmbracoApiControllerBase>();
        }

        /// <summary>
        /// Gets the disabled controllers.
        /// </summary>
        public TypeList<UmbracoApiControllerBase> Controllers { get; }

        /// <summary>
        /// Disables the device preview feature of previewing.
        /// </summary>
        public bool DisableDevicePreview { get; set; }
        
        /// <summary>
        /// If true, all references to templates will be removed in the back office and routing
        /// </summary>
        public bool DisableTemplates { get; set; }

    }
}
