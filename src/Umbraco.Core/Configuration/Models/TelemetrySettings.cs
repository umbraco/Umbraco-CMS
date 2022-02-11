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
        /// Gets the system information metrics.
        /// </summary>
        /// <value>
        /// The system information metrics.
        /// </value>
        public IDictionary<SystemInformationMetric, bool> SystemInformation { get; } = new Dictionary<SystemInformationMetric, bool>();

        /// <summary>
        /// Gets the usage information metrics.
        /// </summary>
        /// <value>
        /// The usage information metrics.
        /// </value>
        public IDictionary<UsageInformationMetric, bool> UsageInformation { get; } = new Dictionary<UsageInformationMetric, bool>();
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
        /// Sets the metrics based on the specified telemetry level.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="level">The telemetry level.</param>
        public static void Set(this TelemetrySettings telemetrySettings, TelemetryLevel level)
        {
            if (level != TelemetryLevel.Off)
            {
                // Set default basic telemetry metrics
                telemetrySettings.TrySet(SystemInformationMetric.UmbracoVersion, true);
                telemetrySettings.TrySet(SystemInformationMetric.PackageVersions, true);
            }

            if (level == TelemetryLevel.Detailed)
            {
                // Set detailed telemetry metrics
                foreach (SystemInformationMetric metric in Enum.GetValues(typeof(SystemInformationMetric)))
                {
                    telemetrySettings.TrySet(metric, true);
                }

                foreach (UsageInformationMetric metric in Enum.GetValues(typeof(UsageInformationMetric)))
                {
                    telemetrySettings.TrySet(metric, true);
                }
            }
        }

        /// <summary>
        /// Sets the specified metric.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void Set(this TelemetrySettings telemetrySettings, SystemInformationMetric metric, bool value)
            => telemetrySettings.SystemInformation[metric] = value;

        /// <summary>
        /// Sets the specified metric.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void Set(this TelemetrySettings telemetrySettings, UsageInformationMetric metric, bool value)
            => telemetrySettings.UsageInformation[metric] = value;

        /// <summary>
        /// Sets the specified metric if it hasn't already been set.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void TrySet(this TelemetrySettings telemetrySettings, SystemInformationMetric metric, bool value)
        {
            if (!telemetrySettings.SystemInformation.ContainsKey(metric))
            {
                telemetrySettings.SystemInformation[metric] = value;
            }
        }

        /// <summary>
        /// Sets the specified metric if it hasn't already been set.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <param name="value">If set to <c>true</c> enables the metric.</param>
        public static void TrySet(this TelemetrySettings telemetrySettings, UsageInformationMetric metric, bool value)
        {
            if (!telemetrySettings.UsageInformation.ContainsKey(metric))
            {
                telemetrySettings.UsageInformation[metric] = value;
            }
        }

        /// <summary>
        /// Determines whether any metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <returns>
        ///   <c>true</c> if any metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings)
        {
            foreach (SystemInformationMetric metric in Enum.GetValues(typeof(SystemInformationMetric)))
            {
                if (telemetrySettings.IsEnabled(metric))
                {
                    return true;
                }
            }

            foreach (UsageInformationMetric metric in Enum.GetValues(typeof(UsageInformationMetric)))
            {
                if (telemetrySettings.IsEnabled(metric))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <returns>
        ///   <c>true</c> if the specified metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings, SystemInformationMetric metric)
            => telemetrySettings.SystemInformation.TryGetValue(metric, out bool value) && value;

        /// <summary>
        /// Determines whether the specified metric is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="metric">The metric.</param>
        /// <returns>
        ///   <c>true</c> if the specified metric is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings, UsageInformationMetric metric)
            => telemetrySettings.UsageInformation.TryGetValue(metric, out bool value) && value;
    }
}
