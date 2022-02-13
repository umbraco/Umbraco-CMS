using System.Linq;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Extension methods for <see cref="TelemetrySettings" />.
    /// </summary>
    public static class TelemetrySettingsExtensions
    {
        /// <summary>
        /// Sets the telemetry data to collect based on the specified telemetry level.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="level">The telemetry level.</param>
        public static void Set(this TelemetrySettings telemetrySettings, TelemetryLevel level)
        {
            foreach (TelemetryData telemetryData in level.GetTelemetryData())
            {
                telemetrySettings.TrySet(telemetryData, true);
            }
        }

        /// <summary>
        /// Sets whether to collect the specified telemetry data.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="telemetryData">The telemetry data.</param>
        /// <param name="value">If set to <c>true</c> enables collecting the telemetry data.</param>
        public static void Set(this TelemetrySettings telemetrySettings, TelemetryData telemetryData, bool value)
            => telemetrySettings.Data[telemetryData] = value;

        /// <summary>
        /// Sets whether to collect the specified telemetry data if it hasn't already been set.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="telemetryData">The telemetry data.</param>
        /// <param name="value">If set to <c>true</c> enables collecting the telemetry data.</param>
        public static void TrySet(this TelemetrySettings telemetrySettings, TelemetryData telemetryData, bool value)
        {
            if (!telemetrySettings.Data.ContainsKey(telemetryData))
            {
                telemetrySettings.Data[telemetryData] = value;
            }
        }

        /// <summary>
        /// Determines whether collecting any telemetry data is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <returns>
        ///   <c>true</c> if collecting any telemetry data is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings)
            => telemetrySettings.Data.Values.Any(x => x);

        /// <summary>
        /// Determines whether collecting the specified telemetry data is enabled.
        /// </summary>
        /// <param name="telemetrySettings">The telemetry settings.</param>
        /// <param name="telemetryData">The telemetry data.</param>
        /// <returns>
        ///   <c>true</c> if collecting the specified telemetry data is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled(this TelemetrySettings telemetrySettings, TelemetryData telemetryData)
            => telemetrySettings.Data.TryGetValue(telemetryData, out bool value) && value;
    }
}
