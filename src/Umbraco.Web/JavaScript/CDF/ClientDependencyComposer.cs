using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.JavaScript.CDF
{
    public sealed class ClientDependencyComposer : ComponentComposer<ClientDependencyComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.RegisterUnique<IRuntimeMinifier, ClientDependencyRuntimeMinifier>();
        }
    }
}
