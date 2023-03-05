using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Search;

/// <summary>
///
/// </summary>
///
[DataContract(Name = "umbracoSearchResult", Namespace = "")]
public class UmbracoSearchResult : IUmbracoSearchResult
{
    public UmbracoSearchResult(string id, float score, IReadOnlyDictionary<string, IReadOnlyList<string>> values)
    {
        Id = id;
        Score = score;
        Values = values.ToDictionary(value => value.Key,
            value => (IList<object>)value.Value.Select(x => (object)x).ToList());
    }

    public UmbracoSearchResult(string id, float score, IReadOnlyDictionary<string, string> values)
    {
        Id = id;
        Score = score;
        Values = values.ToDictionary(value => value.Key, value => (IList<object>)new List<object> { value.Value });
    }

    public string Id { get; set; }
    public float Score { get; set; }
    public int FieldCount => Values.Count();
    public IReadOnlyDictionary<string, IList<object>> Values { get; set; }
}

public interface IUmbracoSearchResult
{
    [DataMember(Name = "id")] public string Id { get; set; }
    [DataMember(Name = "fieldCount")] public int FieldCount => Values.Count();
    [DataMember(Name = "score")] public float Score { get; set; }
    [DataMember(Name = "values")] public IReadOnlyDictionary<string, IList<object>> Values { get; set; }
}
