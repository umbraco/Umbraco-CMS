using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents Umbraco features that can be disabled
    /// </summary>
    internal class DisabledFeatures
    {
        public DisabledFeatures()
        {
            Controllers = new TypeList<UmbracoApiControllerBase>();
        }

        public TypeList<UmbracoApiControllerBase> Controllers { get; private set; }
    }
}