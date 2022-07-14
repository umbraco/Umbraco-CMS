using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Security;

public class BackOfficeIdentityBuilder : IdentityBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeIdentityBuilder" /> class.
    /// </summary>
    public BackOfficeIdentityBuilder(IServiceCollection services)
        : base(typeof(BackOfficeIdentityUser), services)
        => InitializeServices(services);

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeIdentityBuilder" /> class.
    /// </summary>
    public BackOfficeIdentityBuilder(Type role, IServiceCollection services)
        : base(typeof(BackOfficeIdentityUser), role, services)
        => InitializeServices(services);

    // override to add itself, by default identity only wants a single IdentityErrorDescriber
    public override IdentityBuilder AddErrorDescriber<TDescriber>()
    {
        if (!typeof(BackOfficeErrorDescriber).IsAssignableFrom(typeof(TDescriber)))
        {
            throw new InvalidOperationException(
                $"The type {typeof(TDescriber)} does not inherit from {typeof(BackOfficeErrorDescriber)}");
        }

        Services.AddScoped<TDescriber>();
        return this;
    }

    private void InitializeServices(IServiceCollection services)
    {
        // We need to manually register some identity services here because we cannot rely on normal
        // AddIdentity calls for back office users
        // For example: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/IdentityServiceCollectionExtensions.cs#L33
        // The reason we need our own is because the Identity system doesn't cater easily for multiple identity systems and particularly being
        // able to configure IdentityOptions to a specific provider since there is no named options. So we have strongly typed options
        // and strongly typed ILookupNormalizer and IdentityErrorDescriber since those are 'global' and we need to be unintrusive.

        // Services used by identity
        services.AddScoped<IUserValidator<BackOfficeIdentityUser>, UserValidator<BackOfficeIdentityUser>>();
        services.AddScoped<IPasswordValidator<BackOfficeIdentityUser>, PasswordValidator<BackOfficeIdentityUser>>();
        services.AddScoped<IPasswordHasher<BackOfficeIdentityUser>>(
            services => new BackOfficePasswordHasher(
                new LegacyPasswordSecurity(),
                services.GetRequiredService<IJsonSerializer>()));
        services
            .AddScoped<IUserConfirmation<BackOfficeIdentityUser>, UmbracoUserConfirmation<BackOfficeIdentityUser>>();
    }

    /// <summary>
    ///     Adds a token provider for the <seealso cref="BackOfficeIdentityUser" />.
    /// </summary>
    /// <param name="providerName">The name of the provider to add.</param>
    /// <param name="provider">The type of the <see cref="IUserTwoFactorTokenProvider{BackOfficeIdentityUser}" /> to add.</param>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public override IdentityBuilder AddTokenProvider(string providerName, Type provider)
    {
        if (!typeof(IUserTwoFactorTokenProvider<>).MakeGenericType(UserType).GetTypeInfo()
                .IsAssignableFrom(provider.GetTypeInfo()))
        {
            throw new InvalidOperationException($"Invalid Type for TokenProvider: {provider.FullName}");
        }

        Services.Configure<BackOfficeIdentityOptions>(options =>
            options.Tokens.ProviderMap[providerName] = new TokenProviderDescriptor(provider));
        Services.AddTransient(provider);
        return this;
    }
}
