using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "searchResult", Namespace = "")]
public class SearchResultEntity : EntityBasic
{
    /// <summary>
    ///     The score of the search result
    /// </summary>
    [DataMember(Name = "score")]
    public float Score { get; set; }
}
