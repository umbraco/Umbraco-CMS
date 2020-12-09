using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Header
{
    /// <summary>
    /// Represents a header app.
    /// </summary>
    /// <remarks>
    /// <para>Header apps are extensions for in the header.</para>
    /// </remarks>
    [DataContract(Name = "headerApp", Namespace = "")]
    public class HeaderApp
    {
        /// <summary>
        /// Gets the name of the content app.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the unique alias of the header app.
        /// </summary>
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the weight of the header app.
        /// </summary>
        /// <remarks>
        /// <para>Header apps are ordered by weight, from left (lowest values) to right (highest values).</para>
        /// <para>Some built-in apps have special weights: search is -200, help is -100.</para>
        /// <para>The default weight is 0, meaning somewhere after the system defaults, but weight could
        /// be used for ordering between user-level apps, or anything really.</para>
        [DataMember(Name = "weight")]
        public int Weight { get; set; }

        /// <summary>
        /// Gets the icon of the header app.
        /// </summary>
        /// <remarks>
        /// <para>Must be a valid helveticons class name (see http://hlvticons.ch/).</para>
        /// </remarks>
        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets the view for rendering the icon of the header app.
        /// </summary>
        [DataMember(Name = "view")]
        public string View { get; set; }

        /// <summary>
        /// Gets or sets the header app badge.
        /// </summary>
        [DataMember(Name = "badge")]
        public HeaderAppBadge Badge { get; set; }
    }
}
