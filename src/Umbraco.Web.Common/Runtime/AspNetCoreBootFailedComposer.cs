using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Runtime
{
    /// <summary>
    /// Executes if the boot fails to ensure critical services are registered
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.BootFailed)]
    public class AspNetCoreBootFailedComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.Services.AddUnique<BootFailedMiddleware>();
        }
    }
}
