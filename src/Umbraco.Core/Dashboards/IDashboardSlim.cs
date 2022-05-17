using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Dashboards;

/// <summary>
///     Represents a dashboard with only minimal data.
/// </summary>
public interface IDashboardSlim
{
    /// <summary>
    ///     Gets the alias of the dashboard.
    /// </summary>
    [DataMember(Name = "alias")]
    string? Alias { get; }

    /// <summary>
    ///     Gets the view used to render the dashboard.
    /// </summary>
    [DataMember(Name = "view")]
    string? View { get; }
}
