using System;
using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    public interface IUmbracoMiddlewareBuilder
    {
        IRuntimeState RuntimeState { get; }
        IServiceProvider ApplicationServices { get; }
        IApplicationBuilder AppBuilder { get; }
    }
}
