using Umbraco.Core.Composing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Mvc
{
    public class SurfaceControllerTypeCollectionBuilder : TypeCollectionBuilderBase<SurfaceControllerTypeCollectionBuilder, SurfaceControllerTypeCollection, SurfaceController>
    {
        protected override SurfaceControllerTypeCollectionBuilder This => this;
    }
}
