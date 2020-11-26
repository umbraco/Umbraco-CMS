using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;
using Umbraco.Core.WebAssets;

namespace Umbraco.Web.WebAssets.CDF
{
    public sealed class ClientDependencyComposer : ComponentComposer<ClientDependencyComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Services.AddUnique<IRuntimeMinifier, ClientDependencyRuntimeMinifier>();
        }
    }
}
