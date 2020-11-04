using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.WebAssets.CDF
{
    public sealed class ClientDependencyComposer : ComponentComposer<ClientDependencyComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.Services.AddUnique<IRuntimeMinifier, ClientDependencyRuntimeMinifier>();
        }
    }
}
