using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Builder
{

    public interface IUmbracoBuilder
    {
        IServiceCollection Services { get; }
        IWebHostEnvironment WebHostEnvironment { get; }
        IConfiguration Config { get; }
        IUmbracoBuilder AddWith(string key, Action add);
        TBuilder WithCollectionBuilder<TBuilder>() where TBuilder : ICollectionBuilder, new();
        void Build();
    }
}
