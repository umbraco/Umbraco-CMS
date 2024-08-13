using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class UserGroupsBuilderExtensions
{
    internal static IUmbracoBuilder AddUserGroups(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserGroupPresentationFactory, UserGroupPresentationFactory>();
        builder.Services.AddSingleton<IPermissionPresentationFactory, PermissionPresentationFactory>();


        builder.Services.AddSingleton<DocumentPermissionMapper>();
        builder.Services.AddSingleton<IPermissionMapper>(x=>x.GetRequiredService<DocumentPermissionMapper>());
        builder.Services.AddSingleton<IPermissionPresentationMapper>(x=>x.GetRequiredService<DocumentPermissionMapper>());

        return builder;
    }
}
