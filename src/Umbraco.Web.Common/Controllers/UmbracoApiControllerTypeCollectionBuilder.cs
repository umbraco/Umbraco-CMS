using Umbraco.Core.Composing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Common.Controllers
{
    public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
    {
        // TODO: Should this only exist in the back office project? These really are only ever used for the back office AFAIK

        protected override UmbracoApiControllerTypeCollectionBuilder This => this;
    }
}
