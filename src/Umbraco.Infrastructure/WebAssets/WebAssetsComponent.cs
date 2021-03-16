using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.WebAssets
{
    public sealed class WebAssetsComponent : IComponent
    {
        private readonly BackOfficeWebAssets _backOfficeWebAssets;

        public WebAssetsComponent(BackOfficeWebAssets backOfficeWebAssets)
        {
            _backOfficeWebAssets = backOfficeWebAssets;
        }

        public void Initialize()
        {
            // TODO: This will eagerly scan types but we don't really want that, however it works for now.
            // We don't actually have to change Smidge or anything, all we have to do is postpone this call for when the first request on the website arrives.
            _backOfficeWebAssets.CreateBundles();
        }

        public void Terminate()
        {
        }
    }
}
