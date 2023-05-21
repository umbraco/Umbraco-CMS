using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Telemetry.Models;

/// <summary>
///     Serializable class containing telemetry information.
/// </summary>
[DataContract]
public class TelemetryReportData
{
    /// <summary>
    ///     Gets or sets a random GUID to prevent an instance posting multiple times pr. day.
    /// </summary>
    [DataMember(Name = "id")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the Umbraco CMS version.
    /// </summary>
    [DataMember(Name = "version")]
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets an enumerable containing information about packages.
    /// </summary>
    /// <remarks>
    ///     Contains only the name and version of the packages, unless no version is specified.
    /// </remarks>
    [DataMember(Name = "packages")]
    public IEnumerable<PackageTelemetry>? Packages { get; set; }

    [DataMember(Name = "detailed")]
    public IEnumerable<UsageInformation>? Detailed { get; set; }
}
