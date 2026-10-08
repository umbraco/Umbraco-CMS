using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Users;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class UsersBuilderExtensions
{
    internal static IUmbracoBuilder AddUsers(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserPresentationFactory, UserPresentationFactory>();
        builder.Services.TryAddSingleton<ISessionExpiryAccessor, HttpContextSessionExpiryAccessor>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<UsersViewModelsMapDefinition>()
            .Add<CurrentUserViewModelsMapDefinition>();

        return builder;
    }
}
