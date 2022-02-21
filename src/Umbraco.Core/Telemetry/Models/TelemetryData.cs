using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Represents telemetry data to collect.
    /// </summary>
    [DataContract]
    public enum TelemetryData
    {
        /// <summary>
        /// The telemetry identifier (to help correlate multiple reports).
        /// </summary>
        [EnumMember(Value = "id")]
        TelemetryId,

        /// <summary>
        /// Process network information (IP location and network ASN).
        /// </summary>
        [EnumMember(Value = "network")]
        Network,

        /// <summary>
        /// The Umbraco version.
        /// </summary>
        [EnumMember(Value = "version")]
        UmbracoVersion,

        /// <summary>
        /// The installed packages and versions (that allow telemetry data to be reported).
        /// </summary>
        [EnumMember(Value = "packages")]
        PackageVersions,

        /// <summary>
        /// The operating system and version.
        /// </summary>
        [EnumMember(Value = "os")]
        OS,

        /// <summary>
        /// The operating system architecture.
        /// </summary>
        [EnumMember(Value = "osArch")]
        OSArchitecture,

        /// <summary>
        /// The process architecture.
        /// </summary>
        [EnumMember(Value = "processArch")]
        ProcessArchitecture,

        /// <summary>
        /// The .NET runtime version.
        /// </summary>
        [EnumMember(Value = "framework")]
        Framework,

        /// <summary>
        /// The hosting server.
        /// </summary>
        [EnumMember(Value = "server")]
        Server,

        /// <summary>
        /// The ASP.NET Core environment name.
        /// </summary>
        [EnumMember(Value = "environmentName")]
        EnvironmentName,

        /// <summary>
        /// The Umbraco runtime level.
        /// </summary>
        [EnumMember(Value = "runtimeLevel")]
        RuntimeLevel,

        /// <summary>
        /// The total amount of content (draft and published).
        /// </summary>
        [EnumMember(Value = "content")]
        ContentCount,

        /// <summary>
        /// The total amount of domains configured (relative and absolute).
        /// </summary>
        [EnumMember(Value = "domains")]
        DomainCount,

        /// <summary>
        /// The total amount of media.
        /// </summary>
        [EnumMember(Value = "media")]
        MediaCount,

        /// <summary>
        /// The total amount of members.
        /// </summary>
        [EnumMember(Value = "members")]
        MemberCount,

        /// <summary>
        /// The languages used (culture, default, mandatory and fallback).
        /// </summary>
        [EnumMember(Value = "languages")]
        Languages,

        /// <summary>
        /// The property editors used.
        /// </summary>
        [EnumMember(Value = "propertyEditors")]
        PropertyEditors,

        /// <summary>
        /// The total amount of macros.
        /// </summary>
        [EnumMember(Value = "macros")]
        MacroCount,

        /// <summary>
        /// The customized global settings (default UI language, HTTPS, timeouts and whether non-default paths are configured).
        /// </summary>
        [EnumMember(Value = "globalSettings")]
        CustomGlobalSettings,

        /// <summary>
        /// The Models Builder mode.
        /// </summary>
        [EnumMember(Value = "modelsBuilderMode")]
        ModelsBuilderMode
    }
}
