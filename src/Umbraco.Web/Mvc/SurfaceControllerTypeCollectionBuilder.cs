using Umbraco.Core.Composing;

namespace Umbraco.Web.Mvc
{
    public class SurfaceControllerTypeCollectionBuilder : TypeCollectionBuilderBase<SurfaceControllerTypeCollectionBuilder, SurfaceControllerTypeCollection, SurfaceController>
    {
        protected override SurfaceControllerTypeCollectionBuilder This => this;
    }
}
