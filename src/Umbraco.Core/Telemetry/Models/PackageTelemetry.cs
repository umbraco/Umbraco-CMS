using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Serializable class containing information about an installed package.
    /// </summary>
    public class PackageTelemetry
    {
        /// <summary>
        /// Gets or sets the name of the installed package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the installed package.
        /// </summary>
        /// <remarks>
        /// This may be an empty string if no version is specified, or if package telemetry has been restricted.
        /// </remarks>
        public string Version { get; set; }
    }
}
