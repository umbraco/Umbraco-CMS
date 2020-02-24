namespace Umbraco.Core.Configuration
{

    /// <summary>
    /// Password configuration
    /// </summary>
    public interface IPasswordConfiguration
    {
        int RequiredLength { get; }        
        bool RequireNonLetterOrDigit { get; }
        bool RequireDigit { get; }
        bool RequireLowercase { get; }
        bool RequireUppercase { get; }

        bool UseLegacyEncoding { get; }
        string HashAlgorithmType { get; }

        // TODO: This doesn't really belong here
        int MaxFailedAccessAttemptsBeforeLockout { get; }
    }
}
