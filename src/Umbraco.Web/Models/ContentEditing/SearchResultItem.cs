using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "searchResult", Namespace = "")]
    public class SearchResultItem : EntityBasic
    {
        /// <summary>
        /// The score of the search result
        /// </summary>
        [DataMember(Name = "score")]
        public float Score { get; set; }

    }
}
