using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TreeBuilderExtensions
{
    internal static IUmbracoBuilder AddTrees(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserStartNodeEntitiesService, UserStartNodeEntitiesService>();

        return builder;
    }
}
