using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Factories;
<<<<<<< HEAD
=======
using Umbraco.New.Cms.Core.Factories;
>>>>>>> new-backoffice/new_controllers

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class FactoryBuilderExtensions
{
    internal static IUmbracoBuilder AddFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IPagedViewModelFactory, PagedViewModelFactory>();
<<<<<<< HEAD
        builder.Services.AddTransient<IRelationViewModelFactory, RelationViewModelFactory>();
=======
        builder.Services.AddTransient<IDictionaryFactory, DictionaryFactory>();
>>>>>>> new-backoffice/new_controllers
        return builder;
    }

}
