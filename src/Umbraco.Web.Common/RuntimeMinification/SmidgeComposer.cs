using Microsoft.Extensions.DependencyInjection;
using Smidge.FileProcessors;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.Common.RuntimeMinification
{
    public sealed class SmidgeComposer : IComposer
    {
        public void Compose(Composition composition)
        {

            // TODO: For this to work we need to have services.AddSmidge() based on the Smidge APIs but our composer APIs don't really let us do that
            // This makes it a bit awkward to boot the runtime since that call would need to be made outside of the composer... .hrm...


            composition.RegisterUnique<IRuntimeMinifier, SmidgeRuntimeMinifier>();
            composition.RegisterUnique<SmidgeHelperAccessor>();
            composition.Services.AddTransient<IPreProcessor, SmidgeNuglifyJs>();
        }
    }
}
