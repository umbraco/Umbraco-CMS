using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Website.ViewEngines
{
    public class RenderMvcViewOptionsSetup : IConfigureOptions<MvcViewOptions>
    {
        private readonly IRenderViewEngine _renderViewEngine;

        public RenderMvcViewOptionsSetup(IRenderViewEngine renderViewEngine)
        {
            _renderViewEngine = renderViewEngine ?? throw new ArgumentNullException(nameof(renderViewEngine));
        }

        public void Configure(MvcViewOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.ViewEngines.Add(_renderViewEngine);
        }
    }
}
