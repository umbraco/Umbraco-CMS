using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Serializable class containing telemetry information.
    /// </summary>
    [DataContract]
    public class TelemetryReportData
    {
        /// <summary>
        /// Gets or sets a random GUID to prevent an instance posting multiple times pr. day.
        /// </summary>
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Umbraco CMS version.
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets an enumerable containing information about packages.
        /// </summary>
        /// <remarks>
        /// Contains only the name and version of the packages, unless no version is specified.
        /// </remarks>
        [DataMember(Name = "packages")]
        public IEnumerable<PackageTelemetry> Packages { get; set; }

        /// <summary>
        /// Gets the telemetry data.
        /// </summary>
        /// <value>
        /// The telemetry data.
        /// </value>
        [DataMember(Name = "data")]
        public IDictionary<TelemetryData, object> Data { get; } = new Dictionary<TelemetryData, object>();
    }
}
