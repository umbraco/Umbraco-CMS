using Umbraco.Core.Composing;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Web.Website.Collections
{
    public class SurfaceControllerTypeCollectionBuilder : TypeCollectionBuilderBase<SurfaceControllerTypeCollectionBuilder, SurfaceControllerTypeCollection, SurfaceController>
    {
        protected override SurfaceControllerTypeCollectionBuilder This => this;
    }
}
