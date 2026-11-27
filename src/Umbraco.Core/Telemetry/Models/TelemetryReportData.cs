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
    ///     <para>
    ///         Increase this when the shape of this payload changes, so the receiving end can tell old and new payloads apart.
    ///         That means a top-level field (<c>id</c>, <c>version</c>, <c>packages</c>, <c>detailed</c>) is renamed, removed,
    ///         or changes type, or the item shape of <c>packages</c> or <c>detailed</c> changes.
    ///     </para>
    ///     <para>
    ///         Do not increase it for keys added to or changed inside <c>detailed</c>. Each key belongs to the provider that
    ///         sends it. A provider should not change the type or meaning of a key it already sends; it should send a new key
    ///         instead (for example <c>MyPackageStoresV2</c>).
    ///     </para>
    ///     <para>
    ///         Payloads from versions of Umbraco that do not send a schema version have the same shape as version 1.
    ///     </para>
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
