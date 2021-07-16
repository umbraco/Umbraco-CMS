using System;
using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    public interface IUmbracoApplicationBuilderServices
    {
        IApplicationBuilder AppBuilder { get; }
        IServiceProvider ApplicationServices { get; }
        IRuntimeState RuntimeState { get; }
    }
}
