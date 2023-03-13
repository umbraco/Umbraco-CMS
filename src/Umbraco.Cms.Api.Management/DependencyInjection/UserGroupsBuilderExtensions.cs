using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class UserGroupsBuilderExtensions
{
    internal static IUmbracoBuilder AddUserGroups(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserGroupPresentationFactory, UserGroupPresentationFactory>();
        return builder;
    }
}
