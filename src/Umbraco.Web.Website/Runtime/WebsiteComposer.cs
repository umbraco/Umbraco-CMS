using Umbraco.Core.Composing;
using Umbraco.Extensions;
using Umbraco.Web.Common.Runtime;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Web.Website.Runtime
{
    // web's initial composer composes after core's, and before all core composers
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(AspNetCoreComposer))]
    public class WebsiteComposer : IComposer
    {
        public void Compose(Composition composition)
        {

            composition
                .ComposeWebsiteUmbracoControllers()
                //.SetDefaultRenderMvcController<RenderMvcController>()// default controller for template views
                ;

        }
    }
}

