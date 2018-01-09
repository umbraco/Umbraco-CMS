using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model representing a tour.
    /// </summary>
    [DataContract(Name = "tour", Namespace = "")]
    public class Tour
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "alias")]
        public string Alias { get; set; }
        [DataMember(Name = "group")]
        public string Group { get; set; }
        [DataMember(Name = "groupOrder")]
        public int GroupOrder { get; set; }
        [DataMember(Name = "steps")]
        public TourStep[] Steps { get; set; }
    }
}