using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Infrastructure.Security
{
    public class MembersIdentityBuilder : IdentityBuilder
    {
        public MembersIdentityBuilder(IServiceCollection services) : base(typeof(MembersIdentityUser), services)
        {
        }

        public MembersIdentityBuilder(Type role, IServiceCollection services) : base(typeof(MembersIdentityUser), role, services)
        {
        }

        /// <summary>
        /// Adds a token provider for the <seealso cref="MembersIdentityUser"/>.
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
            Services.Configure<MembersIdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider);
            });
            Services.AddTransient(provider);
            return this;
        }
    }
}
