using Umbraco.Core.Composing;

namespace Umbraco.Web.WebApi
{
    public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
    {
        protected override UmbracoApiControllerTypeCollectionBuilder This => this;
    }
}
