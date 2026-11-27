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
    ///     The current version of the telemetry payload shape.
    /// </summary>
    /// <remarks>
    ///     Increase this when the shape of the payload changes, so the receiving end can tell old and new payloads apart.
    ///     Payloads from versions of Umbraco that do not send a schema version have the same shape as version 1.
    /// </remarks>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    ///     Gets or sets the version of the payload shape.
    /// </summary>
    [DataMember(Name = "schemaVersion")]
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

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

    /// <summary>
    ///     Gets or sets detailed telemetry usage information.
    /// </summary>
    [DataMember(Name = "detailed")]
    public IEnumerable<UsageInformation>? Detailed { get; set; }
}
