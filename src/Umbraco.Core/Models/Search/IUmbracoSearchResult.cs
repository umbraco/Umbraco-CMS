namespace Umbraco.Cms.Core.Models.Search;

/// <summary>
///
/// </summary>
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
        Values = values.ToDictionary(value => value.Key, value => (IList<object>)new List<object> { value });
    }

    public string Id { get; set; }
    public float Score { get; set; }
    public IReadOnlyDictionary<string, IList<object>> Values { get; set; }
}

public interface IUmbracoSearchResult
{
    public string Id { get; set; }

    public float Score { get; set; }
    public IReadOnlyDictionary<string, IList<object>> Values { get; set; }
}
