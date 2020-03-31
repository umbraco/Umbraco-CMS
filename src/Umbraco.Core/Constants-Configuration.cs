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
            public const string ConfigSecurityPrefix = ConfigPrefix+"Security:";
            public const string ConfigModelsBuilderPrefix = ConfigPrefix+"ModelsBuilder:";
            public const string ConfigRuntimeMinification = ConfigPrefix+"RuntimeMinification";
            public const string ConfigRuntimeMinificationVersion = ConfigRuntimeMinification+":Version";
        }
    }
}
