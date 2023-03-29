using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class UsersBuilderExtensions
{
    internal static IUmbracoBuilder AddUsers(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserPresentationFactory, UserPresentationFactory>();
        return builder;
    }
}
