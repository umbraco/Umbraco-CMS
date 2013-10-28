using System.Runtime.Serialization;
using Examine;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "searchResult", Namespace = "")]
    public class EntityTypeSearchResult
    {
        [DataMember(Name = "type")]
        public string EntityType { get; set; }

        [DataMember(Name = "results")]
        public ISearchResults Results { get; set; }
    }
}