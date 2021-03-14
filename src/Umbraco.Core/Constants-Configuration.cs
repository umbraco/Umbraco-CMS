﻿namespace Umbraco.Cms.Core
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
            public const string ConfigContentPrefix = ConfigPrefix + "Content:";
            public const string ConfigContentNotificationsPrefix = ConfigContentPrefix + "Notifications:";
            public const string ConfigCorePrefix = ConfigPrefix + "Core:";
            public const string ConfigCustomErrorsPrefix = ConfigPrefix + "CustomErrors:";
            public const string ConfigGlobalPrefix = ConfigPrefix + "Global:";
            public const string ConfigGlobalId = ConfigGlobalPrefix + "Id";
            public const string ConfigHostingPrefix = ConfigPrefix + "Hosting:";
            public const string ConfigModelsBuilderPrefix = ConfigPrefix + "ModelsBuilder:";
            public const string ConfigSecurityPrefix = ConfigPrefix + "Security:";
            public const string ConfigContentNotificationsEmail = ConfigContentNotificationsPrefix + "Email";
            public const string ConfigContentMacroErrors = ConfigContentPrefix + "MacroErrors";
            public const string ConfigGlobalUseHttps = ConfigGlobalPrefix + "UseHttps";
            public const string ConfigHostingDebug = ConfigHostingPrefix + "Debug";
            public const string ConfigCustomErrorsMode = ConfigCustomErrorsPrefix + "Mode";
            public const string ConfigActiveDirectory = ConfigPrefix + "ActiveDirectory";
            public const string ConfigContent = ConfigPrefix + "Content";
            public const string ConfigCoreDebug = ConfigCorePrefix + "Debug";
            public const string ConfigExceptionFilter = ConfigPrefix + "ExceptionFilter";
            public const string ConfigGlobal = ConfigPrefix + "Global";
            public const string ConfigUnattended = ConfigPrefix + "Unattended";
            public const string ConfigHealthChecks = ConfigPrefix + "HealthChecks";
            public const string ConfigHosting = ConfigPrefix + "Hosting";
            public const string ConfigImaging = ConfigPrefix + "Imaging";
            public const string ConfigExamine = ConfigPrefix + "Examine";
            public const string ConfigKeepAlive = ConfigPrefix + "KeepAlive";
            public const string ConfigLogging = ConfigPrefix + "Logging";
            public const string ConfigMemberPassword = ConfigPrefix + "Security:MemberPassword";
            public const string ConfigModelsBuilder = ConfigPrefix + "ModelsBuilder";
            public const string ConfigNuCache = ConfigPrefix + "NuCache";
            public const string ConfigPlugins = ConfigPrefix + "Plugins";
            public const string ConfigRequestHandler = ConfigPrefix + "RequestHandler";
            public const string ConfigRuntime = ConfigPrefix + "Runtime";
            public const string ConfigRuntimeMinification = ConfigPrefix + "RuntimeMinification";
            public const string ConfigRuntimeMinificationVersion = ConfigRuntimeMinification + ":Version";
            public const string ConfigSecurity = ConfigPrefix + "Security";
            public const string ConfigTours = ConfigPrefix + "Tours";
            public const string ConfigTypeFinder = ConfigPrefix + "TypeFinder";
            public const string ConfigWebRouting = ConfigPrefix + "WebRouting";
            public const string ConfigUserPassword = ConfigPrefix + "Security:UserPassword";
            public const string ConfigRichTextEditor = ConfigPrefix + "RichTextEditor";
        }
    }
}
