namespace Umbraco.Cms.Core.Models.Search;

public interface IUmbracoSearchResult
{
    public int Id { get; set; }

    public float Score { get; set; }
    public IDictionary<string, IList<object>> Values { get; set; }
}
