using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<
    UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
{
    // TODO: Should this only exist in the back office project? These really are only ever used for the back office AFAIK
    protected override UmbracoApiControllerTypeCollectionBuilder This => this;
}
