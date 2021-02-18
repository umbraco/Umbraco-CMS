using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Web.WebApi
{
    // TODO: This is moved to netcore so can be deleted when possible

    public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
    {
        protected override UmbracoApiControllerTypeCollectionBuilder This => this;
    }
}
