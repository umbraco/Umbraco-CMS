using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Members;

namespace Umbraco.Infrastructure.Members
{
    public class UmbracoMembersIdentityBuilder : IdentityBuilder
    {

        public UmbracoMembersIdentityBuilder(IServiceCollection services) : base(typeof(UmbracoMembersIdentityUser), services)
        {
        }

        public UmbracoMembersIdentityBuilder(Type role, IServiceCollection services) : base(typeof(UmbracoMembersIdentityUser), role, services)
        {
        }

        /// <summary>
        /// Adds a token provider for the <seealso cref="UmbracoMembersIdentityUser"/>.
        /// </summary>
        /// <param name="providerName">The name of the provider to add.</param>
        /// <param name="provider">The type of the <see cref="IUserTwoFactorTokenProvider{UmbracoMembersIdentityUser}"/> to add.</param>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public override IdentityBuilder AddTokenProvider(string providerName, Type provider)
        {
            if (!typeof(IUserTwoFactorTokenProvider<>).MakeGenericType(UserType).GetTypeInfo().IsAssignableFrom(provider.GetTypeInfo()))
            {
                throw new InvalidOperationException($"Invalid Type for TokenProvider: {provider.FullName}");
            }
            Services.Configure<UmbracoMembersIdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider);
            });
            Services.AddTransient(provider);
            return this;
        }
    }
}
