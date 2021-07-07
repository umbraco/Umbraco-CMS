using System;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Cms.Web.Website.Controllers
{
    public class UmbracoRenderingDefaultsOptionsSetup : IConfigureOptions<UmbracoRenderingDefaultsOptions>
    {
        //private IOptions<UmbracoRenderingDefaultsOptions> _umbracoRenderingDefaultOptions;

        //public UmbracoRenderingDefaultsOptionsSetup(IOptions<UmbracoRenderingDefaultsOptions> umbracoRenderingDefaultOptions)
        //{
        //    _umbracoRenderingDefaultOptions = umbracoRenderingDefaultOptions;
        //}

        public void Configure(UmbracoRenderingDefaultsOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //options.DefaultControllerType = _umbracoRenderingDefaultOptions.Value.DefaultControllerType;

            options.DefaultControllerType = typeof(RenderController);
        }
    }
}
