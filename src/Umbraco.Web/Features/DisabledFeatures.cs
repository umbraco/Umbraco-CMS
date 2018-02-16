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
    }
}
