namespace Umbraco.Core
{
    public static partial class Constants
    {
        public static class Configuration
        {
            /// <summary>
            /// Case insensitive prefix for all configurations
            /// </summary>
            /// <remarks>
            /// ":" is used as marker for nested objects in json. E.g. "Umbraco:CMS:" = {"Umbraco":{"CMS":{....}}
            /// </remarks>
            public const string ConfigPrefix = "Umbraco:CMS:";
            public const string ConfigSecurityPrefix = ConfigPrefix + "Security:";
            public const string ConfigGlobalPrefix = ConfigPrefix + "Global:";
            public const string ConfigContentPrefix = ConfigPrefix + "Content:";
            public const string ConfigModelsBuilderPrefix = ConfigPrefix + "ModelsBuilder:";
            public const string ConfigContentNotificationsPrefix = ConfigContentPrefix + "Notifications:";
            public const string ConfigHostingPrefix = ConfigPrefix + "Hosting:";
            public const string ConfigCustomErrorsPrefix = ConfigPrefix + "CustomErrors:";

            public const string ConfigRuntimeMinification = ConfigPrefix + "RuntimeMinification:";
            public const string ConfigRuntimeMinificationVersion = ConfigRuntimeMinification + "Version";
            public const string ConfigContentNotificationsEmail = ConfigContentNotificationsPrefix + "Email";
            public const string ConfigContentMacroErrors = ConfigContentPrefix + "MacroErrors";
            public const string ConfigGlobalUseHttps = ConfigGlobalPrefix + "UseHttps";
            public const string ConfigHostingDebug = ConfigHostingPrefix + "Debug";
            public const string ConfigCustomErrorsMode = ConfigCustomErrorsPrefix + "Mode";
        }
    }
}
