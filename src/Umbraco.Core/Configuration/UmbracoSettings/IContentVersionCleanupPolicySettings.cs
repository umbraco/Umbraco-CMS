namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentVersionCleanupPolicySettings
    {
        bool EnableCleanup { get; }
        int KeepAllVersionsNewerThanDays { get; }
        int KeepLatestVersionPerDayForDays { get; }
    }
}
