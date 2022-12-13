using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class FactoryBuilderExtensions
{
    internal static IUmbracoBuilder AddFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IModelsBuilderViewModelFactory, ModelsBuilderViewModelFactory>();
        builder.Services.AddTransient<IRelationViewModelFactory, RelationViewModelFactory>();
        builder.Services.AddTransient<IDictionaryFactory, DictionaryFactory>();
        builder.Services.AddTransient<IRedirectUrlStatusViewModelFactory, RedirectUrlStatusViewModelFactory>();
        builder.Services.AddTransient<IRedirectUrlViewModelFactory, RedirectUrlViewModelFactory>();
        return builder;
    }
}
