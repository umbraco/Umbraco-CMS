using System;
using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    public interface IUmbracoApplicationBuilder
    {
        IRuntimeState RuntimeState { get; }
        IServiceProvider ApplicationServices { get; }
        IApplicationBuilder AppBuilder { get; }

        /// <summary>
        /// Final call during app building to configure endpoints
        /// </summary>
        /// <param name="configureUmbraco"></param>
        void WithEndpoints(Action<IUmbracoEndpointBuilder> configureUmbraco);
    }
}
