using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A model representing the file used to load a tour.
/// </summary>
[DataContract(Name = "tourFile", Namespace = "")]
public class BackOfficeTourFile
{
    public BackOfficeTourFile() => Tours = new List<BackOfficeTour>();

    /// <summary>
    ///     The file name for the tour
    /// </summary>
    [DataMember(Name = "fileName")]
    public string? FileName { get; set; }

    /// <summary>
    ///     The plugin folder that the tour comes from
    /// </summary>
    /// <remarks>
    ///     If this is null it means it's a Core tour
    /// </remarks>
    [DataMember(Name = "pluginName")]
    public string? PluginName { get; set; }

    [DataMember(Name = "tours")]
    public IEnumerable<BackOfficeTour> Tours { get; set; }
}
