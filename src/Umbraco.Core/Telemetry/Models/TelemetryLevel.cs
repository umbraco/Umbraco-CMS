namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Represents a predifined telemetry level.
    /// </summary>
    public enum TelemetryLevel
    {
        /// <summary>
        /// Turn telemetry off.
        /// </summary>
        Off,

        /// <summary>
        /// Turn on basic telemetry.
        /// </summary>
        Basic,

        /// <summary>
        /// Turn on detailed telemetry.
        /// </summary>
        Detailed
    }
}
