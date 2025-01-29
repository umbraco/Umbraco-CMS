using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

[Obsolete("This will be removed in Umbraco 15.")]
public class UmbracoApiControllerTypeCollectionBuilder : TypeCollectionBuilderBase<
    UmbracoApiControllerTypeCollectionBuilder, UmbracoApiControllerTypeCollection, UmbracoApiController>
{
    protected override UmbracoApiControllerTypeCollectionBuilder This => this;
}
