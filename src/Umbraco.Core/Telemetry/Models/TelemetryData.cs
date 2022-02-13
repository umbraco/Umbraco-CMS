namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Represents telemetry data to collect.
    /// </summary>
    public enum TelemetryData
    {
        /// <summary>
        /// The Umbraco version.
        /// </summary>
        UmbracoVersion,

        /// <summary>
        /// The installed packages and versions (that allow telemetry data to be reported).
        /// </summary>
        PackageVersions,

        /// <summary>
        /// The operating system and version.
        /// </summary>
        OS,

        /// <summary>
        /// The operating system architecture.
        /// </summary>
        OSArchitecture,

        /// <summary>
        /// The process architecture.
        /// </summary>
        ProcessArchitecture,

        /// <summary>
        /// The .NET runtime version.
        /// </summary>
        Framework,

        /// <summary>
        /// The hosting server and version.
        /// </summary>
        // TODO: Implement a collector
        Server,

        /// <summary>
        /// The ASP.NET Core environment name.
        /// </summary>
        EnvironmentName,

        /// <summary>
        /// The total amount of content (draft and published).
        /// </summary>
        ContentCount,

        /// <summary>
        /// The total amount of domains configured (relative and absolute).
        /// </summary>
        DomainCount,

        /// <summary>
        /// The total amount of media.
        /// </summary>
        MediaCount,

        /// <summary>
        /// The total amount of members.
        /// </summary>
        MemberCount,

        /// <summary>
        /// The languages used (culture, default, mandatory and fallback).
        /// </summary>
        Languages,

        /// <summary>
        /// The property editors used.
        /// </summary>
        PropertyEditors,

        /// <summary>
        /// The total amount of macros.
        /// </summary>
        MacroCount
    }
}
