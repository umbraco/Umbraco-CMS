using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.WebAssets
{
    public sealed class WebAssetsComposer : ComponentComposer<WebAssetsComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Services.AddUnique<BackOfficeWebAssets>();
        }
    }
}
