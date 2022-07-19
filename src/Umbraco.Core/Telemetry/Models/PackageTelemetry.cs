using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Telemetry.Models;

/// <summary>
///     Serializable class containing information about an installed package.
/// </summary>
[DataContract(Name = "packageTelemetry")]
public class PackageTelemetry
{
    /// <summary>
    ///     Gets or sets the name of the installed package.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the version of the installed package.
    /// </summary>
    /// <remarks>
    ///     This may be an empty string if no version is specified, or if package telemetry has been restricted.
    /// </remarks>
    [DataMember(Name = "version")]
    public string? Version { get; set; }
}
