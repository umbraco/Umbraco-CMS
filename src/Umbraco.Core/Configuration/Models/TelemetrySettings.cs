using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Configuration settings for Umbraco telemetry data.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigTelemetry)]
    public class TelemetrySettings
    {
        /// <summary>
        /// Gets or sets a predifined telemetry level.
        /// </summary>
        /// <value>
        /// The telemetry level.
        /// </value>
        public TelemetryLevel? Level { get; set; }

        /// <summary>
        /// Gets or sets the system information metrics.
        /// </summary>
        /// <value>
        /// The system information metrics.
        /// </value>
        public IDictionary<SystemInformationMetric, bool> SystemInformation { get; set; }

        /// <summary>
        /// Gets or sets the usage information metrics.
        /// </summary>
        /// <value>
        /// The usage information metrics.
        /// </value>
        public IDictionary<UsageInformationMetric, bool> UsageInformation { get; set; }
    }

    /// <summary>
    /// Represents a predifined telemetry level.
    /// </summary>
    public enum TelemetryLevel
    {
        /// <summary>
        /// Keep all metrics off.
        /// </summary>
        Off,

        /// <summary>
        /// Only turn on basic metrics.
        /// </summary>
        Basic,

        /// <summary>
        /// Turn on all metrics.
        /// </summary>
        Detailed
    }

    /// <summary>
    /// Represents a system information metric.
    /// </summary>
    public enum SystemInformationMetric
    {
        /// <summary>
        /// The operating system and version.
        /// </summary>
        OS,

        /// <summary>
        /// The .NET runtime version.
        /// </summary>
        Framework,

        /// <summary>
        /// The hosting server and version.
        /// </summary>
        Server,

        /// <summary>
        /// The ASP.NET Core environment name.
        /// </summary>
        EnvironmentName,

        /// <summary>
        /// The Umbraco version.
        /// </summary>
        UmbracoVersion,

        /// <summary>
        /// The installed packages and versions (that allow telemetry data to be reported).
        /// </summary>
        PackageVersions
    }

    /// <summary>
    /// Represents an usage information metric.
    /// </summary>
    public enum UsageInformationMetric
    {
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
        /// The languages used (culture, default and fallback).
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

    /// <summary>
    /// Extension methods for setting telemetry metrics.
    /// </summary>
    public static class TelemetrySettingsExtensions
    {
        /// <summary>
        /// Determines whether any metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <returns>
        ///   <c>true</c> if any metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings)
        {
            foreach (SystemInformationMetric key in Enum.GetValues(typeof(SystemInformationMetric)))
            {
                if (telemetrySettings.IsEnabled(key))
                {
                    return true;
                }
            }

            foreach (UsageInformationMetric key in Enum.GetValues(typeof(UsageInformationMetric)))
            {
                if (telemetrySettings.IsEnabled(key))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the metrics based on the specified telemetry level.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="level">The telemetry level.</param>
        public static void Set(this TelemetrySettings telemetrySettings, TelemetryLevel level)
        {
            if (level != TelemetryLevel.Off)
            {
                // Set default basic telemetry metrics
                telemetrySettings.Set(SystemInformationMetric.UmbracoVersion, true);
                telemetrySettings.Set(SystemInformationMetric.PackageVersions, true);
            }

            if (level == TelemetryLevel.Detailed)
            {
                // Set detailed telemetry metrics
                foreach (SystemInformationMetric key in Enum.GetValues(typeof(SystemInformationMetric)))
                {
                    telemetrySettings.Set(key, true);
                }

                foreach (UsageInformationMetric key in Enum.GetValues(typeof(UsageInformationMetric)))
                {
                    telemetrySettings.Set(key, true);
                }
            }
        }

        /// <summary>
        /// Sets the specified metric.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="key">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void Set(this TelemetrySettings telemetrySettings, SystemInformationMetric key, bool value)
        {
            telemetrySettings.SystemInformation ??= new Dictionary<SystemInformationMetric, bool>();
            telemetrySettings.SystemInformation[key] = value;
        }

        /// <summary>
        /// Sets the specified metric.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="key">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void Set(this TelemetrySettings telemetrySettings, UsageInformationMetric key, bool value)
        {
            telemetrySettings.UsageInformation ??= new Dictionary<UsageInformationMetric, bool>();
            telemetrySettings.UsageInformation[key] = value;
        }

        /// <summary>
        /// Determines whether the specified metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="key">The metric.</param>
        /// <returns>
        ///   <c>true</c> if the specified metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings, SystemInformationMetric key)
            => telemetrySettings.SystemInformation?.TryGetValue(key, out bool value) == true && value;

        /// <summary>
        /// Determines whether the specified metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="key">The metric.</param>
        /// <returns>
        ///   <c>true</c> if the specified metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings, UsageInformationMetric key)
            => telemetrySettings.UsageInformation?.TryGetValue(key, out bool value) == true && value;
    }
}
