using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Factories;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class FactoryBuilderExtensions
{
    internal static IUmbracoBuilder AddFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IPagedViewModelFactory, PagedViewModelFactory>();
        builder.Services.AddTransient<IRelationViewModelFactory, RelationViewModelFactory>();
        return builder;
    }

}
