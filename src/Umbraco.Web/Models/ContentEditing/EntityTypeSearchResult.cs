using System.Collections.Generic;
using System.Runtime.Serialization;
using Examine;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a search result by entity type
    /// </summary>
    [DataContract(Name = "searchResult", Namespace = "")]
    public class EntityTypeSearchResult
    {
        [DataMember(Name = "type")]
        public string EntityType { get; set; }

        [DataMember(Name = "results")]
        public IEnumerable<EntityBasic> Results { get; set; }
    }
}