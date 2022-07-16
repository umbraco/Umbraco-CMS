namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

public interface IPasswordConfigurationSection : IUmbracoConfigurationSection
{
    int RequiredLength { get; }

    bool RequireNonLetterOrDigit { get; }

    bool RequireDigit { get; }

    bool RequireLowercase { get; }

    bool RequireUppercase { get; }

    bool UseLegacyEncoding { get; }

    string HashAlgorithmType { get; }

    int MaxFailedAccessAttemptsBeforeLockout { get; }
}
