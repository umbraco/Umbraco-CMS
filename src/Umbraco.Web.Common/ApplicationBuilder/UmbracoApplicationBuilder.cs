using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// A builder to allow encapsulating the enabled routing features in Umbraco
    /// </summary>
    internal class UmbracoApplicationBuilder : IUmbracoApplicationBuilder
    {
        public UmbracoApplicationBuilder(IApplicationBuilder appBuilder)
        {
            AppBuilder = appBuilder;
        }

        public IApplicationBuilder AppBuilder { get; }
    }
}
