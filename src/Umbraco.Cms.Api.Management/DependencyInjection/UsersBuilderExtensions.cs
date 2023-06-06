using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Users;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class UsersBuilderExtensions
{
    internal static IUmbracoBuilder AddUsers(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserPresentationFactory, UserPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<UsersViewModelsMapDefinition>()
            .Add<CurrentUserViewModelsMapDefinition>();

        return builder;
    }
}
