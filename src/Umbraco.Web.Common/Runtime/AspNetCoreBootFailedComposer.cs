using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Runtime
{
    /// <summary>
    /// Executes if the boot fails to ensure critical services are registered
    /// </summary>
    public class AspNetCoreBootFailedComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<BootFailedMiddleware>();
        }
    }
}
