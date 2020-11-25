using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Builder
{
    public interface IUmbracoBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Config { get; }
        TypeLoader TypeLoader { get; }
        ILoggerFactory BuilderLoggerFactory { get; }
        TBuilder WithCollectionBuilder<TBuilder>() where TBuilder : ICollectionBuilder, new();
        void Build();
    }
}
