using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Umbraco.Web.Common.Controllers
{
    /// <summary>
    /// A request feature to allowing proxying viewdata from one controller to another
    /// </summary>
    public sealed class ProxyViewDataFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyViewDataFeature"/> class.
        /// </summary>
        public ProxyViewDataFeature(ViewDataDictionary viewData) => ViewData = viewData;

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>
        /// </summary>
        public ViewDataDictionary ViewData { get; }
    }
}
