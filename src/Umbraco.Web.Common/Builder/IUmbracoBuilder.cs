using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;

namespace Umbraco.Web.Common.Builder
{

    public interface IUmbracoBuilder
    {
        IServiceCollection Services { get; }
        IWebHostEnvironment WebHostEnvironment { get; }
        IConfiguration Config { get; }
        IUmbracoBuilder AddWith(string key, Action add);
        void Build();
    }
}
