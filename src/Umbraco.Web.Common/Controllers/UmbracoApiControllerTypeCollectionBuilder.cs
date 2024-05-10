using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<
    UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
{
    protected override UmbracoApiControllerTypeCollectionBuilder This => this;
}
