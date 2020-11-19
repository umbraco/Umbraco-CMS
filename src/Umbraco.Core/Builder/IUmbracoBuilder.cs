using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Builder
{
    public interface IUmbracoBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Config { get; }
        TypeLoader TypeLoader { get; set; } // TODO: Remove setter, see note on concrete
        TBuilder WithCollectionBuilder<TBuilder>() where TBuilder : ICollectionBuilder, new();
        void Build();
    }
}
