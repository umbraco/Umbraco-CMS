namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class StartNodeFilter
{
    public IEnumerable<string> AnyOfDocTypeAlias { get; set; } = Array.Empty<string>(); // empty is all / *
    public string DirectionAlias { get; set; } = string.Empty;
}
