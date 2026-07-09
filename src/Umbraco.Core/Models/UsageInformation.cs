using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents usage information data for telemetry or reporting purposes.
/// </summary>
[DataContract]
public class UsageInformation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UsageInformation" /> class.
    /// </summary>
    /// <param name="name">The name identifying the type of usage information.</param>
    /// <param name="data">The usage data payload.</param>
    public UsageInformation(string name, object data)
    {
        Name = name;
        Data = data;
    }

    /// <summary>
    ///     Gets the name identifying the type of usage information.
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; }

    /// <summary>
    ///     Gets the usage data payload.
    /// </summary>
    [DataMember(Name = "data")]
    public object Data { get; }
}
