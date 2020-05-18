using Umbraco.Core.Composing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Common.Controllers
{
    public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
    {
        protected override UmbracoApiControllerTypeCollectionBuilder This => this;
    }
}
