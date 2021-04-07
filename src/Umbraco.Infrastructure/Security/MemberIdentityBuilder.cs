using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.Security
{
    public class MemberIdentityBuilder : IdentityBuilder
    {
        public MemberIdentityBuilder(IServiceCollection services) : base(typeof(MemberIdentityUser), services)
        {
        }

        public MemberIdentityBuilder(Type role, IServiceCollection services) : base(typeof(MemberIdentityUser), role, services)
        {
        }

        /// <summary>
        /// Adds a token provider for the <seealso cref="MemberIdentityUser"/>.
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
            Services.Configure<MemberIdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider);
            });
            Services.AddTransient(provider);
            return this;
        }
    }
}
