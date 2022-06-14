using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Dashboards;

/// <summary>
///     Represents a dashboard.
/// </summary>
public interface IDashboard : IDashboardSlim
{
    /// <summary>
    ///     Gets the aliases of sections/applications where this dashboard appears.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This field is *not* needed by the UI and therefore we want to exclude
    ///         it from serialization, but it is deserialized as part of the manifest,
    ///         therefore we cannot plainly ignore it.
    ///     </para>
    ///     <para>
    ///         So, it has to remain a data member, plus we use our special
    ///         JsonDontSerialize attribute (see attribute for more details).
    ///     </para>
    /// </remarks>
    [DataMember(Name = "sections")]
    string[] Sections { get; }

    /// <summary>
    ///     Gets the access rule determining the visibility of the dashboard.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This field is *not* needed by the UI and therefore we want to exclude
    ///         it from serialization, but it is deserialized as part of the manifest,
    ///         therefore we cannot plainly ignore it.
    ///     </para>
    ///     <para>
    ///         So, it has to remain a data member, plus we use our special
    ///         JsonDontSerialize attribute (see attribute for more details).
    ///     </para>
    /// </remarks>
    [DataMember(Name = "access")]
    IAccessRule[] AccessRules { get; }
}
