using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "tourFile", Namespace = "")]
    public class BackOfficeTourFile
    {
        /// <summary>
        /// The file name for the tour
        /// </summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// The plugin folder that the tour comes from
        /// </summary>
        /// <remarks>
        /// If this is null it means it's a Core tour
        /// </remarks>
        [DataMember(Name = "pluginName")]
        public string PluginName { get; set; }

        [DataMember(Name = "tours")]
        public IEnumerable<BackOfficeTour> Tours { get; set; }
    }
}