namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentVersionCleanupPolicyGlobalSettings
    {
        bool EnableCleanup { get; }
        int KeepAllVersionsNewerThanDays { get; }
        int KeepLatestVersionPerDayForDays { get; }
    }
}
