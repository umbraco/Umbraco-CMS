using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Security;

namespace Umbraco.Infrastructure.BackOffice
{
    public class BackOfficeIdentityBuilder : IdentityBuilder
    {
        public BackOfficeIdentityBuilder(IServiceCollection services) : base(typeof(BackOfficeIdentityUser), services)
        {
        }

        public BackOfficeIdentityBuilder(Type role, IServiceCollection services) : base(typeof(BackOfficeIdentityUser), role, services)
        {
        }

        /// <summary>
        /// Adds a token provider for the <seealso cref="BackOfficeIdentityUser"/>.
        /// </summary>
        /// <param name="providerName">The name of the provider to add.</param>
        /// <param name="provider">The type of the <see cref="IUserTwoFactorTokenProvider{BackOfficeIdentityUser}"/> to add.</param>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public override IdentityBuilder AddTokenProvider(string providerName, Type provider)
        {
            if (!typeof(IUserTwoFactorTokenProvider<>).MakeGenericType(UserType).GetTypeInfo().IsAssignableFrom(provider.GetTypeInfo()))
            {
                throw new InvalidOperationException($"Invalid Type for TokenProvider: {provider.FullName}");
            }
            Services.Configure<BackOfficeIdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider);
            });
            Services.AddTransient(provider);
            return this;
        }
    }
}
