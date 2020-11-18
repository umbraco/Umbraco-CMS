using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Extensions;
using Umbraco.Web.Website.Routing;
using Umbraco.Web.Common.Runtime;
using Umbraco.Web.Website.Collections;

namespace Umbraco.Web.Website.Runtime
{
    // web's initial composer composes after core's, and before all core composers
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(AspNetCoreComposer))]
    public class WebsiteComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.Services.AddUnique<NoContentRoutes>();

            composition.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                 .Add(composition.TypeLoader.GetSurfaceControllers());
        }
    }
}

