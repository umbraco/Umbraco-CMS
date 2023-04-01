using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Telemetry.Models;

/// <summary>
/// Serializable class containing information about an installed package.
/// </summary>
[DataContract(Name = "packageTelemetry")]
public class PackageTelemetry
{
    /// <summary>
    /// Gets or sets the identifier of the installed package.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    [DataMember(Name = "id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the installed package.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the installed package.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    [DataMember(Name = "version")]
    public string? Version { get; set; }
}
