using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class RelationTypesBuilderExtensions
{
    internal static IUmbracoBuilder AddRelationTypes(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IObjectTypeViewModelFactory, ObjectTypeViewModelFactory>();
        return builder;
    }
}
