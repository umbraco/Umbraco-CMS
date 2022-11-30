using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Web.Website.Collections;

public class SurfaceControllerTypeCollectionBuilder : TypeCollectionBuilderBase<SurfaceControllerTypeCollectionBuilder,
    SurfaceControllerTypeCollection, SurfaceController>
{
    protected override SurfaceControllerTypeCollectionBuilder This => this;
}
