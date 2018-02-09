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
        public TypeList<UmbracoApiControllerBase> Controllers { get; private set; }

        /// <summary>
        /// Specifies if rendering pipeline should ignore HasTemplate check when handling a request.
        /// <remarks>This is to allow JSON preview of content with no template set.</remarks>
        /// </summary>
        public bool AllowRenderWithoutTemplate { get; set; }
    }
}
