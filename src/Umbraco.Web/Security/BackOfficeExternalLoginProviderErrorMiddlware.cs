using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin;
using Newtonsoft.Json;
using Umbraco.Core;

namespace Umbraco.Web.Security
{
    internal class BackOfficeExternalLoginProviderErrorMiddlware : OwinMiddleware
    {
        public BackOfficeExternalLoginProviderErrorMiddlware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!context.Request.Uri.IsClientSideRequest())
            {
                // check if we have any errors registered
                var errors = context.GetExternalLoginProviderErrors();
                if (errors != null)
                {
                    // this is pretty nasty to resolve this from the MVC service locator but that's all we can really work with since that is where it is
                    var tempDataProvider = DependencyResolver.Current.GetService<ITempDataProvider>();

                    // create a 'fake' controller context for temp data to work. we want to use temp data because it's self managing and we won't have to
                    // deal with resetting anything and plus it's configurable (by default uses session). better than creating a state manager ourselves.
                    var controllerContext = new ControllerContext(
                        context.TryGetHttpContext().Result,
                        new System.Web.Routing.RouteData(),
                        new EmptyController());

                    tempDataProvider.SaveTempData(controllerContext, new Dictionary<string, object>
                    {
                        [ViewDataExtensions.TokenExternalSignInError] = errors
                    });
                }
            }

            if (Next != null)
            {
                await Next.Invoke(context);
            }
        }

        private class EmptyController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
