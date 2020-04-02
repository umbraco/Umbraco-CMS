using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.WebAssets;
using Umbraco.Web.JavaScript;
using Umbraco.Web.PropertyEditors;
using Umbraco.Infrastructure.WebAssets;

namespace Umbraco.Web.WebAssets
{
    public sealed class WebAssetsComposer : ComponentComposer<WebAssetsComponent>
    {
    }

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
