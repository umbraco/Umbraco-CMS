using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.Security;

public class MemberIdentityBuilder : IdentityBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityBuilder" /> class.
    /// </summary>
    public MemberIdentityBuilder(IServiceCollection services)
        : base(typeof(MemberIdentityUser), services)
        => InitializeServices(services);

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberIdentityBuilder" /> class.
    /// </summary>
    public MemberIdentityBuilder(Type role, IServiceCollection services)
        : base(typeof(MemberIdentityUser), role, services)
        => InitializeServices(services);

    // override to add itself, by default identity only wants a single IdentityErrorDescriber
    public override IdentityBuilder AddErrorDescriber<TDescriber>()
    {
        if (!typeof(MembersErrorDescriber).IsAssignableFrom(typeof(TDescriber)))
        {
            throw new InvalidOperationException(
                $"The type {typeof(TDescriber)} does not inherit from {typeof(MembersErrorDescriber)}");
        }

        Services.AddScoped<TDescriber>();
        return this;
    }

    private void InitializeServices(IServiceCollection services)
    {
    }

    /// <summary>
    ///     Adds a token provider for the <seealso cref="MemberIdentityBuilder" />.
    /// </summary>
    /// <param name="providerName">The name of the provider to add.</param>
    /// <param name="provider">The type of the <see cref="IUserTwoFactorTokenProvider{MemberIdentityBuilder}" /> to add.</param>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public override IdentityBuilder AddTokenProvider(string providerName, Type provider)
    {
        if (!typeof(IUserTwoFactorTokenProvider<>).MakeGenericType(UserType).GetTypeInfo()
                .IsAssignableFrom(provider.GetTypeInfo()))
        {
            throw new InvalidOperationException($"Invalid Type for TokenProvider: {provider.FullName}");
        }

        Services.Configure<IdentityOptions>(options =>
            options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider));
        Services.AddTransient(provider);
        return this;
    }
}
