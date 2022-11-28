namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines the constants used for the Umbraco package repository
    /// </summary>
    [Obsolete("This is no longer used and will be removed in Umbraco 13")]
    public static class PackageRepository
    {
        public const string RestApiBaseUrl = "https://our.umbraco.com/webapi/packages/v1";
        public const string DefaultRepositoryName = "Umbraco package Repository";
        public const string DefaultRepositoryId = "65194810-1f85-11dd-bd0b-0800200c9a66";
    }
}
