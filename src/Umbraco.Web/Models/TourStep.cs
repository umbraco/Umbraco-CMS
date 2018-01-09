using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model representing a step in a tour.
    /// </summary>
    [DataContract(Name = "step", Namespace = "")]
    public class TourStep
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "content")]
        public string Content { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}