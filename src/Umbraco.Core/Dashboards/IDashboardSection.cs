using System.Runtime.Serialization;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Dashboards
{
    public interface IDashboardSection
    {
        /// <summary>
        /// Display name of the dashboard tab
        /// </summary>
        [DataMember(Name="name")]
        string Name { get; }

        /// <summary>
        /// Alias to refer to this dashboard via code
        /// </summary>
        [DataMember(Name = "alias")]
        string Alias { get; }

        /// <summary>
        /// A collection of sections/application aliases that this dashboard will appear on
        /// </summary>
        [DataMember(Name = "sections")]
        [IgnoreDataMemberWhenSerializing]
        string[] Sections { get; }

        /// <summary>
        /// The HTML view to load for the dashboard
        /// </summary>
        [DataMember(Name = "view")]
        string View { get; }

        /// <summary>
        /// Dashboards can be shown/hidden based on access rights
        /// </summary>
        [DataMember(Name = "access")]
        [IgnoreDataMemberWhenSerializing]
        IAccessRule[] AccessRules { get; }
    }
}
