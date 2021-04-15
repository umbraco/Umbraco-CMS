using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// A builder to allow encapsulating the enabled routing features in Umbraco
    /// </summary>
    public interface IUmbracoApplicationBuilder
    {
        /// <summary>
        /// Returns the <see cref="IApplicationBuilder"/>
        /// </summary>
        IApplicationBuilder AppBuilder { get; }
    }
}
